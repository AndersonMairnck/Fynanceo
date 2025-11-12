// Controllers/GestaoMesasController.cs
using Fynanceo.Migrations;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.PedidosModel;
using Microsoft.AspNetCore.Mvc;

namespace Fynanceo.Controllers
{
    public class GestaoMesasController : Controller
    {
        private readonly IMesaService _mesaService;
        private readonly IPedidoService _pedidoService;
        private readonly IProdutoService _produtoService;

        public GestaoMesasController(
            IMesaService mesaService,
            IPedidoService pedidoService,
            IProdutoService produtoService)
        {
            _mesaService = mesaService;
            _pedidoService = pedidoService;
            _produtoService = produtoService;
        }

        // Dashboard principal de mesas
        public async Task<IActionResult> Dashboard()
        {
            var mesas = await _mesaService.ObterTodosAsync();
            var pedidosAtivos = await _pedidoService.ObterPedidosDoDia();

            var viewModel = new DashboardMesasViewModel
            {
                Mesas = mesas,
                PedidosAtivos = pedidosAtivos
            };

            return View(viewModel);
        }

        // Visualizar/gerenciar mesa específica
        public async Task<IActionResult> GerenciarMesa(int id)
        {
            var mesa = await _mesaService.ObterPorIdAsync(id);
            if (mesa == null)
            {
                return NotFound();
            }

            // Buscar pedido ativo da mesa
            var pedidoAtivo = await _pedidoService.ObterPedidoAtivoPorMesa(id);

            var viewModel = new GerenciarMesaViewModel
            {
                Mesa = mesa,
                PedidoAtivo = pedidoAtivo,
                ProdutosDisponiveis = await _produtoService.ObterProdutosPopularesAsync(10),
                CategoriasDisponiveis = await _produtoService.ObterCategoriasAsync()
            };

            return View(viewModel);
        }

        // Abrir nova comanda na mesa
        [HttpPost]
        public async Task<IActionResult> AbrirComanda(int mesaId)
        {
            try
            {
                var pedidoViewModel = new PedidoViewModel
                {
                    TipoPedido = TipoPedido.Mesa,
                    MesaId = mesaId,
                    Status = PedidoStatus.Aberto,
                    DataAbertura = DateTime.UtcNow
                };

                var pedido = await _pedidoService.CriarPedido(pedidoViewModel);

                // Atualizar status da mesa para ocupada
                await _mesaService.AtualizarStatusAsync(mesaId, "Ocupada");

                return Json(new { success = true, pedidoId = pedido.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Adicionar item ao pedido da mesa
        [HttpPost]
        public async Task<IActionResult> AdicionarItemMesa([FromBody] AdicionarItemMesaRequest request)
        {
            try
            {
                var itemVm = new ItemPedidoViewModel
                {
                    ProdutoId = request.ProdutoId,
                    Quantidade = request.Quantidade,
                    Observacoes = request.Observacoes,
                    Personalizacoes = request.Personalizacoes
                };

                await _pedidoService.AdicionarItem(request.PedidoId, itemVm);
               
                return Json(new { success = true, message = "Item adicionado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Enviar pedido para cozinha
        [HttpPost]
        public async Task<IActionResult> EnviarCozinha(int pedidoId)
        {
            try
            {
                var pedido = await _pedidoService.AtualizarStatus(pedidoId, "EnviadoCozinha", User.Identity.Name);
                return Json(new { success = true, message = "Pedido enviado para cozinha!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Buscar pedidos (para o sistema de cards)
        [HttpGet]
        public async Task<IActionResult> BuscarPedidos(string status = "", string tipo = "")
        {
            var pedidos = await _pedidoService.ObterPedidosDoDia();

            // Filtrar se necessário
            if (!string.IsNullOrEmpty(status))
            {
                pedidos = pedidos.Where(p => p.Status.ToString() == status).ToList();
            }

            if (!string.IsNullOrEmpty(tipo))
            {
                pedidos = pedidos.Where(p => p.TipoPedido.ToString() == tipo).ToList();
            }

            return ViewComponent("PedidosCards", new { pedidos = pedidos });
        }


        [HttpGet]
        public async Task<JsonResult> BuscarProdutos(string termo = "", string categoria = "")
        {
            try
            {
                var produtos = await _produtoService.BuscarProdutosAsync(termo, categoria);

                var resultado = produtos.Select(p => new
                {
                    id = p.Id,
                    nome = p.Nome,
                    descricao = p.Descricao,
                    categoria = p.Categoria,
                    valorVenda = p.ValorVenda,
                 
                }).ToList();

                return Json(new { success = true, produtos = resultado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class AdicionarItemMesaRequest
    {
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public string Observacoes { get; set; }
        public string Personalizacoes { get; set; }
    }


}