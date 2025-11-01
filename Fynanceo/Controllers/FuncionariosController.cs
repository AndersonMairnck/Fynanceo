using Microsoft.AspNetCore.Mvc;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.FuncionariosModel;

namespace Fynanceo.Controllers
{
    public class FuncionariosController : Controller
    {
        private readonly IFuncionarioService _funcionarioService;

        public FuncionariosController(IFuncionarioService funcionarioService)
        {
            _funcionarioService = funcionarioService;
        }

        public async Task<IActionResult> Index()
        {
            var funcionarios = await _funcionarioService.ObterTodosAsync();
            return View(funcionarios);
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var funcionario = await _funcionarioService.ObterPorIdAsync(id);
            if (funcionario == null)
            {
                return NotFound();
            }
            return View(funcionario);
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(FuncionarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _funcionarioService.CpfExisteAsync(model.CPF))
                {
                    ModelState.AddModelError("CPF", "CPF já cadastrado.");
                    return View(model);
                }

                var resultado = await _funcionarioService.AdicionarAsync(model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Funcionário cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao cadastrar funcionário. Tente novamente.");
            }
            return View(model);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var funcionario = await _funcionarioService.ObterPorIdAsync(id);
            if (funcionario == null)
            {
                return NotFound();
            }

            var model = new FuncionarioViewModel
            {
                Id = funcionario.Id,
                NomeCompleto = funcionario.NomeCompleto,
              
                CPF = funcionario.CPF,
                Endereco = funcionario.Endereco,
                Telefone = funcionario.Telefone,
                Email = funcionario.Email,
                ContatoEmergencia = funcionario.ContatoEmergencia,
                TelefoneEmergencia = funcionario.TelefoneEmergencia,
                Cargo = funcionario.Cargo,
                NivelPermissao = funcionario.NivelPermissao,
                Turno = funcionario.Turno,
                Ativo = funcionario.Ativo,
                DataAdmissao = funcionario.DataAdmissao
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, FuncionarioViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _funcionarioService.CpfExisteAsync(model.CPF, id))
                {
                    ModelState.AddModelError("CPF", "CPF já cadastrado.");
                    return View(model);
                }

                var resultado = await _funcionarioService.AtualizarAsync(id, model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Funcionário atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao atualizar funcionário. Tente novamente.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var resultado = await _funcionarioService.ExcluirAsync(id);
            if (resultado)
            {
                TempData["Sucesso"] = "Funcionário excluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir funcionário. Tente novamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}