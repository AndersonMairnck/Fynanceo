using Microsoft.AspNetCore.Mvc;
using Fynanceo.Utils;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.ClientesModel;
namespace Fynanceo.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private const int PageSize = 15; // Itens por página

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            // CORREÇÃO: Usando tuple explicitamente
            var resultTuple = await _clienteService.ObterClientesPaginadosAsync(page, PageSize, search);
            var clientes = resultTuple.Clientes;
            var totalCount = resultTuple.TotalCount;
            var totalPages = resultTuple.TotalPages;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.PageSize = PageSize;

            return View(clientes);
        }

        // Novo endpoint para carregar clientes via AJAX
        public async Task<IActionResult> GetClientesPaginados(int page = 1, string search = "")
        {
            try
            {
                // CORREÇÃO: Usando tuple explicitamente
                var resultTuple = await _clienteService.ObterClientesPaginadosAsync(page, PageSize, search);
                var clientes = resultTuple.Clientes;
                var totalCount = resultTuple.TotalCount;
                var totalPages = resultTuple.TotalPages;

                var result = clientes.Select(c => new
                {
                    id = c.Id,
                    nomeCompleto = c.NomeCompleto,
                    cpfCnpj = c.CpfCnpj,
                    telefone = c.Telefone,
                    email = c.Email,
                    classificacao = c.Classificacao,
                    ativo = c.Ativo,
                    dataCadastro = c.DataCadastro.ToString("dd/MM/yyyy"),
                    classificacaoBadge = c.Classificacao == "VIP" ? "bg-warning" :
                                       c.Classificacao == "Frequente" ? "bg-info" : "bg-secondary",
                    statusBadge = c.Ativo ? "bg-success" : "bg-danger",
                    statusText = c.Ativo ? "Ativo" : "Inativo"
                }).ToList();

                return Json(new
                {
                    clientes = result,
                    totalCount,
                    totalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
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