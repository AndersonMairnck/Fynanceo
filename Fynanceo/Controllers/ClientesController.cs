using Microsoft.AspNetCore.Mvc;
using Fynanceo.Services;
using Fynanceo.ViewModels;
using Fynanceo.Utils;
namespace Fynanceo.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _clienteService.ObterTodosAsync();
            return View(clientes);
        }

        // GET: Clientes/Detalhes/5
        public async Task<IActionResult> Detalhes(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // GET: Clientes/Cadastrar
        public IActionResult Cadastrar()
        {
            return View();
        }

        // POST: Clientes/Cadastrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(ClienteViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.CpfCnpj = StringUtils.RemoverCaracteresEspeciais(model.CpfCnpj);  
                if (await _clienteService.CpfCnpjExisteAsync(model.CpfCnpj))
                {
                    ModelState.AddModelError("CpfCnpj", "CPF/CNPJ já cadastrado.");
                    return View(model);
                }

                var resultado = await _clienteService.AdicionarAsync(model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Cliente cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao cadastrar cliente. Tente novamente.");
            }
            return View(model);
        }

        // GET: Clientes/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            var model = new ClienteViewModel
            {
                Id = cliente.Id,
                NomeCompleto = cliente.NomeCompleto,
                CpfCnpj = cliente.CpfCnpj,
                Telefone = cliente.Telefone,
                Email = cliente.Email,
                DataNascimento = cliente.DataNascimento,
                Classificacao = cliente.Classificacao,
                Observacoes = cliente.Observacoes,
                Ativo = cliente.Ativo,
                JustificativaStatus = cliente.JustificativaStatus
            };

            return View(model);
        }

        // POST: Clientes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ClienteViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _clienteService.CpfCnpjExisteAsync(model.CpfCnpj, id))
                {
                    ModelState.AddModelError("CpfCnpj", "CPF/CNPJ já cadastrado.");
                    return View(model);
                }

                var resultado = await _clienteService.AtualizarAsync(id, model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Cliente atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao atualizar cliente. Tente novamente.");
            }
            return View(model);
        }

        // POST: Clientes/Excluir/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var resultado = await _clienteService.ExcluirAsync(id);
            if (resultado)
            {
                TempData["Sucesso"] = "Cliente excluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir cliente. Tente novamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}