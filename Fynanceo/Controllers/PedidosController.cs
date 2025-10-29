// Controllers/PedidosController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.ViewModels;
using Fynanceo.Services;
using Fynanceo.Models.Enums;

namespace Fynanceo.Controllers
{
    public class PedidosController : Controller
    {
       // private readonly ApplicationDbContext _context;
        private readonly IPedidoService _pedidoService;
        private readonly IMesaService _mesaService;
        private readonly IProdutoService _produtoService;
        private readonly IClienteService _clienteService;

        public PedidosController( IPedidoService pedidoService,IMesaService mesaService, IProdutoService produtoService,IClienteService clienteService)
        {
          
            _pedidoService = pedidoService;
            _mesaService = mesaService;
            _produtoService = produtoService;
            _clienteService = clienteService;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var pedidos = await _pedidoService.ObterPedidosDoDia();
            return View(pedidos);
        }

        // GET: Pedidos/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new PedidoViewModel
            {
                MesasDisponiveis = await _mesaService.ObterPorStatusAsync("Livre"),
                ProdutosDisponiveis = await _produtoService.ObterTodosAsync(),
                Clientes = await _clienteService.ObterTodosAsync()

            }; 

            return View(viewModel);
        }

        // POST: Pedidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var pedido = await _pedidoService.CriarPedido(viewModel);

                    //// Adicionar itens se houver
                    //if (viewModel.Itens.Any())
                    //{
                    //    foreach (var item in viewModel.Itens)
                    //    {
                    //        await _pedidoService.AdicionarItem(pedido.Id, item);
                    //    }
                    //}

                    TempData["Success"] = "Pedido criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = pedido.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao criar pedido: {ex.Message}");
                }
            }

            //Recarregar dados se houver erro
            viewModel.MesasDisponiveis = await _mesaService.ObterPorStatusAsync("Livre");
            viewModel.Clientes = await _clienteService.ObterTodosAsync();

            viewModel.ProdutosDisponiveis = await _produtoService.ObterTodosAsync();

            return View(viewModel);
        }

        // GET: Pedidos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var pedido = await _pedidoService.ObterPedidoCompleto(id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // GET: Pedidos/Cozinha
        public async Task<IActionResult> Cozinha()
        {
            var viewModel = new CozinhaViewModel
            {
                PedidosCozinha = await _pedidoService.ObterPedidosPorStatus("EnviadoCozinha"),
                PedidosPreparo = await _pedidoService.ObterPedidosPorStatus("EmPreparo"),
                PedidosProntos = await _pedidoService.ObterPedidosPorStatus("Pronto")
            };

            return View(viewModel); // Agora passamos o ViewModel completo
        }

        // POST: Pedidos/AtualizarStatus/5
        [HttpPost]
        public async Task<IActionResult> AtualizarStatus(int id, string novoStatus)
        {
            try
            {
                var usuario = "Usuário"; // Temporário - depois pegar do usuário logado
                var pedido = await _pedidoService.AtualizarStatus(id, novoStatus, usuario);

                return Json(new { success = true, message = "Status atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Pedidos/GetEnderecosCliente/5
        public async Task<JsonResult> GetEnderecosCliente(int clienteId)
        {
            var enderecos = await _clienteService.ObterPorIdAsync(clienteId);

            return Json(enderecos);
        }

        // POST: Pedidos/AdicionarItem
        [HttpPost]
        public async Task<JsonResult> AdicionarItem([FromBody] ItemPedidoViewModel itemVm)
        {
            try
            {
                var pedido = await _pedidoService.AdicionarItem(itemVm.ProdutoId, itemVm);
                return Json(new { success = true, message = "Item adicionado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // No PedidosController.cs
        [HttpPost]
        public async Task<JsonResult> IniciarPreparoItem(int itemId)
        {
            try
            {
                var item = await _pedidoService.IniciarPreparoItemAsync(itemId);
                if (item == null)
                    return Json(new { success = false, message = "Item não encontrado" });

                return Json(new { success = true, message = "Preparo iniciado", item });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> MarcarProntoItem(int itemId)
        {
            try
            {
                var item = await _pedidoService.MarcarProntoItemAsync(itemId);
                if (item == null)
                    return Json(new { success = false, message = "Item não encontrado" });

                return Json(new { success = true, message = "Item marcado como pronto", item });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> IniciarPreparoTodos(int pedidoId)
        {
            try
            {
                var sucesso = await _pedidoService.IniciarPreparoTodosAsync(pedidoId);

                if (!sucesso)
                {
                    TempData["Mensagem"] = "Nenhum item disponível para iniciar preparo.";
                    TempData["TipoMensagem"] = "erro";
                }
                else
                {
                    TempData["Mensagem"] = "Preparo de todos os itens iniciado com sucesso!";
                    TempData["TipoMensagem"] = "sucesso";
                }

                // Redireciona para a mesma página (ou outra)
                return RedirectToAction("Cozinha");
            }
            catch (Exception ex)
            {
                TempData["Mensagem"] = "Erro ao iniciar preparo: " + ex.Message;
                TempData["TipoMensagem"] = "erro";
                return RedirectToAction("Cozinha");
            }
        }




        [HttpPost]
        public async Task<JsonResult> MarcarProntoTodos(int pedidoId)
        {
            try
            {
                var sucesso = await _pedidoService.MarcarProntoTodosAsync(pedidoId);
                if (sucesso == null)
                    return Json(new { success = false, message = "Nenhum item pendente para marcar como pronto" });

                return Json(new { success = true, message = "Todos os itens marcados como prontos" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}