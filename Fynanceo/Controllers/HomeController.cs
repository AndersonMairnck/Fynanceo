using System.Diagnostics;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.DashboardModel;
using Microsoft.AspNetCore.Mvc;

namespace Fynanceo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPedidoService _pedidoService;

        public HomeController(ILogger<HomeController> logger, IPedidoService pedidoService)
        {
            _logger = logger;
            _pedidoService = pedidoService;
        }

        public async Task<IActionResult> Index()
        {
            // Carrega pedidos do dia
            var pedidosHoje = await _pedidoService.ObterPedidosDoDia();

            // KPIs
            var vendasHoje = pedidosHoje
                .Where(p => p.Status == PedidoStatus.Finalizado || p.Status == PedidoStatus.Fechado)
                .Sum(p => p.Total);

            var pedidosAtivos = pedidosHoje
                .Count(p => p.Status != PedidoStatus.Finalizado && p.Status != PedidoStatus.Fechado && p.Status != PedidoStatus.Cancelado);

            var mesasOcupadas = pedidosHoje
                .Where(p => p.TipoPedido == TipoPedido.Mesa && p.MesaId != null &&
                            p.Status != PedidoStatus.Finalizado && p.Status != PedidoStatus.Fechado && p.Status != PedidoStatus.Cancelado)
                .Select(p => p.MesaId)
                .Distinct()
                .Count();

            var pedidosFechadosHoje = pedidosHoje
                .Count(p => p.Status == PedidoStatus.Finalizado || p.Status == PedidoStatus.Fechado);

            var ticketMedio = pedidosFechadosHoje > 0 ? vendasHoje / pedidosFechadosHoje : 0m;

            var vm = new DashboardViewModel
            {
                VendasHoje = vendasHoje,
                PedidosAtivos = pedidosAtivos,
                MesasOcupadas = mesasOcupadas,
                TicketMedio = ticketMedio,
                UltimaAtualizacao = DateTime.UtcNow
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
