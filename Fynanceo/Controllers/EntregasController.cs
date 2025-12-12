// Controllers/EntregasController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;

using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.DeliveryModel;
using Fynanceo.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Fynanceo.Controllers
{
    [Authorize(Roles = "Administrador, Gerente, Atendente, Entregador")]
    public class EntregasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEntregaService _entregaService;
        private readonly IPedidoService _pedidoService;

        public EntregasController(AppDbContext context, IEntregaService entregaService, IPedidoService pedidoService)
        {
            _context = context;
            _entregaService = entregaService;
            _pedidoService = pedidoService;
        }

        // GET: Entregas
        // public async Task<IActionResult> Index()
        // {
        //     var entregas = await _entregaService.ObterEntregasDoDia();
        //     return View(entregas);
        // }
        public async Task<IActionResult> Index()
        {
            var entregas = await _entregaService.ObterEntregasDoDia();
            var entregadores = entregas
                .Where(e => e.Entregador != null)
                .Select(e => e.Entregador)
                .Distinct()
                .ToList();

            var vm = new EntregasIndexViewModel
            {
                Entregas = entregas,
                Entregadores = entregadores
            };

            return View(vm);
        }

        // GET: Entregas/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _entregaService.ObterDashboardDelivery();
            return View(dashboard);
        }

        // GET: Entregas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entrega = await _context.Entregas
                .Include(e => e.Pedido)
                    .ThenInclude(p => p.Cliente)
                     .Include(e => e.EnderecoEntrega)
                .Include(e => e.Entregador)
                .Include(e => e.Historico)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrega == null)
            {
                return NotFound();
            }

            return View(entrega);
        }

        // GET: Entregas/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new EntregaViewModel
            {
                PedidosPendentes = await _context.Pedidos
                    .Where(p => p.TipoPedido == TipoPedido.Delivery &&
                               p.Status == PedidoStatus.Pronto &&
                               !_context.Entregas.Any(e => e.PedidoId == p.Id))
                    .Include(p => p.Cliente)
                     .Include(p => p.EnderecoEntrega)  // ← ADICIONAR ESTE INCLUDE
            .Include(p => p.Itens)           // ← ADICIONAR ESTE INCLUDE
                .ThenInclude(i => i.Produto) // ← ADICIONAR ESTE INCLUDE
            .ToListAsync(),
                EntregadoresDisponiveis = await _entregaService.ObterEntregadoresDisponiveis()
            };

            return View(viewModel);
        }

        // POST: Entregas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EntregaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Criar entrega a partir do pedido
                    var entrega = await _entregaService.CriarEntrega(viewModel.PedidoId);

                    // Atribuir entregador se selecionado
                    if (viewModel.EntregadorId.HasValue)
                    {
                        entrega = await _entregaService.AtribuirEntregador(entrega.Id, viewModel.EntregadorId.Value);
                    }

                    TempData["Success"] = "Entrega criada com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = entrega.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao criar entrega: {ex.Message}");
                }
            }

            // Recarregar dados se houver erro
            viewModel.PedidosPendentes = await _context.Pedidos
                .Where(p => p.TipoPedido == TipoPedido.Delivery &&
                           p.Status == PedidoStatus.Pronto &&
                           !_context.Entregas.Any(e => e.PedidoId == p.Id))
                .Include(p => p.Cliente)
                .ToListAsync();
            viewModel.EntregadoresDisponiveis = await _entregaService.ObterEntregadoresDisponiveis();

            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<JsonResult> AtribuirEntregador(int id, int entregadorId)
        {
            try
            {
                var entrega = await _entregaService.AtribuirEntregador(id, entregadorId);
                return Json(new { success = true, message = "Entregador atribuído com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Entregas/AtualizarStatus/5
        [HttpPost]
        public async Task<JsonResult> AtualizarStatus(int id, string novoStatus, string? observacao = null)
        {
            try
            {
              
                var entrega = await _entregaService.AtualizarStatusEntrega(id, novoStatus,  observacao);

                return Json(new { success = true, message = "Status atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Entregas/Mapa
        public async Task<IActionResult> Mapa()
        {
            var entregas = await _context.Entregas
                .Include(e => e.Pedido)
                .Include(e => e.Entregador)
                .Where(e => e.Status == StatusEntrega.RetiradoParaEntrega ||
                           e.Status == StatusEntrega.EmRota)
                .ToListAsync();

            var entregadores = await _context.Entregadores
                .Where(e => e.Status == StatusEntregador.Entregando &&
                           e.Latitude.HasValue && e.Longitude.HasValue)
                .ToListAsync();

            ViewBag.Entregas = entregas;
            ViewBag.Entregadores = entregadores;

            return View();
        }

        // GET: Entregas/Pendentes
        public async Task<IActionResult> Pendentes()
        {
            var entregas = await _entregaService.ObterEntregasPorStatus("AguardandoEntregador");
            return View(entregas);
        }

        // GET: Entregas/EmAndamento
        public async Task<IActionResult> EmAndamento()
        {
            
            var entregas = await _context.Entregas
        .Include(e => e.Pedido)
          .Include(e => e.EnderecoEntrega)
            .ThenInclude(p => p.Cliente)
        .Include(e => e.Entregador)
        .Where(e => e.Status == StatusEntrega.RetiradoParaEntrega ||
                   e.Status == StatusEntrega.EmRota)
        .OrderBy(e => e.DataSaiuEntrega)
        .ToListAsync();

            return View(entregas);
        }

        // GET: Entregas/ObterEntregadoresDisponiveis
        [HttpGet]
        public async Task<JsonResult> ObterEntregadoresDisponiveis()
        {
            var entregadores = await _entregaService.ObterEntregadoresDisponiveis();
            var result = entregadores.Select(e => new
            {
                id = e.Id,
                nome = e.Nome,
                telefone = e.Telefone,
                tipoVeiculo = e.TipoVeiculo.ToString(),
                placa = e.Placa
            }).ToList();

            return Json(result);
        }

        // POST: MarcarEmRota mais de um para o mesmo entregador
        [HttpPost]
        public async Task<IActionResult> MarcarEmRota(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return RedirectToAction("Index");

            var listaIds = ids
                .Split(',')
                .Select(id => int.Parse(id))
                .ToList();

            await _entregaService.MarcarComoEmRotaAsync(listaIds);
          
        
          


            TempData["Success"] = $"{listaIds.Count} entrega(s) marcadas como 'Em Rota'.";
            return RedirectToAction("Index");
        }

    }
}