using Fynanceo.Configuracao;
using Fynanceo.Models;
using Fynanceo.ViewModel.IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fynanceo.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UserManager<UsuarioAplicacao> _userManager;
        private readonly SignInManager<UsuarioAplicacao> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(
            UserManager<UsuarioAplicacao> userManager,
            SignInManager<UsuarioAplicacao> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsuarioController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        #region Login/Logout

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Entrar(string? returnUrl = null, bool sessaoExpirada = false)
        {
            if (sessaoExpirada)
            {
                ViewBag.MensagemAlerta = "Sua sessão expirou porque você fez login em outro dispositivo.";
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrar(EntrarViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _userManager.FindByEmailAsync(model.Email);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
                return View(model);
            }

            if (!usuario.Ativo)
            {
                ModelState.AddModelError(string.Empty, 
                    "Sua conta está desativada. Entre em contato com o administrador.");
                return View(model);
            }

            // ✅ Fazer logout de qualquer sessão anterior do mesmo usuário
            if (!string.IsNullOrEmpty(usuario.CurrentSessionId))
            {
                _logger.LogInformation(
                    "Usuário {Email} já possui uma sessão ativa. A sessão anterior será encerrada.",
                    usuario.Email);
            }

            var result = await _signInManager.PasswordSignInAsync(
                usuario,
                model.Senha,
                model.LembrarMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // ✅ Gerar novo SessionId único
                var sessionId = Guid.NewGuid().ToString();

                // ✅ Salvar no banco de dados
                usuario.CurrentSessionId = sessionId;
                usuario.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(usuario);

                // ✅ Salvar na sessão ASP.NET Core (compartilhada entre abas do mesmo navegador)
                HttpContext.Session.SetString("SessionId", sessionId);

                _logger.LogInformation(
                    "Login bem-sucedido para {Email}. SessionId: {SessionId}",
                    usuario.Email,
                    sessionId);

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(usuario);
                var tempoRestante = lockoutEnd.HasValue
                    ? (lockoutEnd.Value - DateTimeOffset.UtcNow).Minutes
                    : 15;

                ModelState.AddModelError(string.Empty,
                    $"Conta bloqueada por excesso de tentativas. Tente novamente em {tempoRestante} minutos.");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, 
                    "Login não permitido. Verifique se seu e-mail foi confirmado.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sair()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // ✅ Limpar SessionId do banco
                user.CurrentSessionId = null;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Logout realizado para {Email}", user.Email);
            }

            // ✅ Limpar sessão
            HttpContext.Session.Clear();

            // ✅ Fazer logout
            await _signInManager.SignOutAsync();

            return RedirectToAction("Entrar");
        }

        #endregion

        #region Registro (Apenas Admin pode registrar novos usuários)
        
        
        
        [HttpGet]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        public async Task<IActionResult> GerenciarPermissoes(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();
    
            var claimsAtuais = await _userManager.GetClaimsAsync(usuario);
            var todasPermissoes = Permissoes.ObterTodasPorCategoria();
    
            ViewBag.Usuario = usuario;
            ViewBag.PermissoesAtuais = claimsAtuais
                .Where(c => c.Type == "Permissao")
                .Select(c => c.Value)
                .ToList();
            ViewBag.TodasPermissoes = todasPermissoes;
    
            return View();
        }

        [HttpPost]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GerenciarPermissoes(string id, List<string> permissoesSelecionadas)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();
    
            // Remover todas as permissões atuais
            var claimsAtuais = await _userManager.GetClaimsAsync(usuario);
            var claimsPermissao = claimsAtuais.Where(c => c.Type == "Permissao").ToList();
    
            if (claimsPermissao.Any())
            {
                await _userManager.RemoveClaimsAsync(usuario, claimsPermissao);
            }
    
            // Adicionar novas permissões selecionadas
            if (permissoesSelecionadas != null && permissoesSelecionadas.Any())
            {
                var novosClaims = permissoesSelecionadas
                    .Select(p => new System.Security.Claims.Claim("Permissao", p))
                    .ToList();
        
                await _userManager.AddClaimsAsync(usuario, novosClaims);
            }
    
            TempData["Sucesso"] = "Permissões atualizadas com sucesso!";
            return RedirectToAction("ListarUsuarios");
        }

        [HttpGet]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        public IActionResult Registrar()
        {
            CarregarPerfis();
            return View();
        }

        [HttpPost]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                CarregarPerfis();
                return View(model);
            }

            var usuario = new UsuarioAplicacao
            {
                UserName = model.Email,
                Email = model.Email,
                NomeCompleto = model.NomeCompleto,
                Cargo = model.Cargo,
                EmailConfirmed = true,
                Ativo = true
            };

            var result = await _userManager.CreateAsync(usuario, model.Senha);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.PerfilSelecionado))
                {
                    await _userManager.AddToRoleAsync(usuario, model.PerfilSelecionado);
                }

                TempData["Sucesso"] = $"Usuário {model.NomeCompleto} cadastrado com sucesso!";
                return RedirectToAction("ListarUsuarios");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            CarregarPerfis();
            return View(model);
        }

        #endregion

        #region Gerenciamento de Usuários

        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = _userManager.Users.ToList();
            var listaUsuarios = new List<dynamic>();

            foreach (var usuario in usuarios)
            {
                var perfis = await _userManager.GetRolesAsync(usuario);
                listaUsuarios.Add(new
                {
                    usuario.Id,
                    usuario.NomeCompleto,
                    usuario.Email,
                    usuario.Cargo,
                    usuario.Ativo,
                    usuario.DataCadastro,
                    Perfis = string.Join(", ", perfis)
                });
            }

            return View(listaUsuarios);
        }

        [HttpGet]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        public async Task<IActionResult> EditarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound();

            var perfisUsuario = await _userManager.GetRolesAsync(usuario);

            ViewBag.Perfis = new SelectList(
                _roleManager.Roles.Select(r => r.Name).ToList(),
                perfisUsuario.FirstOrDefault());

            return View(usuario);
        }

        [HttpPost]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(string id, UsuarioAplicacao model, string perfilSelecionado)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound();

            usuario.NomeCompleto = model.NomeCompleto;
            usuario.Cargo = model.Cargo;
            usuario.Ativo = model.Ativo;

            var result = await _userManager.UpdateAsync(usuario);

            if (result.Succeeded)
            {
                // Atualizar perfis
                var perfisAtuais = await _userManager.GetRolesAsync(usuario);
                await _userManager.RemoveFromRolesAsync(usuario, perfisAtuais);

                if (!string.IsNullOrEmpty(perfilSelecionado))
                {
                    await _userManager.AddToRoleAsync(usuario, perfilSelecionado);
                }

                TempData["Sucesso"] = "Usuário atualizado com sucesso!";
                return RedirectToAction("ListarUsuarios");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Perfis = new SelectList(
                _roleManager.Roles.Select(r => r.Name).ToList(),
                perfilSelecionado);

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesativarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound();

            usuario.Ativo = false;
            await _userManager.UpdateAsync(usuario);

            TempData["Sucesso"] = "Usuário desativado com sucesso!";
            return RedirectToAction("ListarUsuarios");
        }

        [HttpPost]
        [Authorize(Policy = Politicas.GerenciarUsuarios)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtivarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound();

            usuario.Ativo = true;
            await _userManager.UpdateAsync(usuario);

            TempData["Sucesso"] = "Usuário ativado com sucesso!";
            return RedirectToAction("ListarUsuarios");
        }

        #endregion

        #region Alterar Senha

        [HttpGet]
        [Authorize]
        public IActionResult AlterarSenha()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return RedirectToAction("Entrar");

            var result = await _userManager.ChangePasswordAsync(usuario, model.SenhaAtual, model.NovaSenha);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(usuario);
                TempData["Sucesso"] = "Senha alterada com sucesso!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        #endregion

        #region Acesso Negado

        [AllowAnonymous]
        public IActionResult AcessoNegado()
        {
            return View();
        }

        #endregion

        #region Helpers

        private void CarregarPerfis()
        {
            ViewBag.Perfis = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
