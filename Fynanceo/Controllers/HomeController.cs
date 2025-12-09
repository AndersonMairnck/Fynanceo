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

        // Endpoints JSON para o dashboard moderno
        [HttpGet]
        public async Task<IActionResult> VendasPorHoraHoje()
        {
            var pedidosHoje = await _pedidoService.ObterPedidosDoDia();

            // Considera apenas pedidos fechados/finalizados para vendas
            var relevantes = pedidosHoje
                .Where(p => p.Status == PedidoStatus.Finalizado || p.Status == PedidoStatus.Fechado)
                .ToList();

            // Monta 24 horas do dia local
            var inicioLocal = DateTime.Now.Date; // início do dia local
            var inicioUtc = inicioLocal.ToUniversalTime();

            var porHora = Enumerable.Range(0, 24)
                .Select(h => new
                {
                    hora = h,
                    total = relevantes
                        .Where(p =>
                        {
                            var dt = p.DataFechamento ?? p.DataAbertura;
                            var local = dt.ToLocalTime();
                            return local.Date == inicioLocal && local.Hour == h;
                        })
                        .Sum(p => p.Total)
                })
                .ToList();

            return Json(porHora);
        }

        [HttpGet]
        public async Task<IActionResult> PedidosStatusHoje()
        {
            var pedidosHoje = await _pedidoService.ObterPedidosDoDia();

            var dados = Enum.GetValues(typeof(PedidoStatus))
                .Cast<PedidoStatus>()
                .Select(s => new
                {
                    status = s.ToString(),
                    quantidade = pedidosHoje.Count(p => p.Status == s)
                });

            return Json(dados);
        }

        [HttpGet]
        public async Task<IActionResult> UltimosPedidos(int take = 10)
        {
            var pedidosHoje = await _pedidoService.ObterPedidosDoDia();

            var lista = pedidosHoje
                .OrderByDescending(p => p.DataAbertura)
                .Take(Math.Clamp(take, 1, 50))
                .Select(p => new
                {
                    p.Id,
                    p.NumeroPedido,
                    tipo = p.TipoPedido.ToString(),
                    status = p.Status.ToString(),
                    total = p.Total,
                    abertura = (p.DataAbertura).ToLocalTime(),
                    fechamento = (p.DataFechamento).GetValueOrDefault().ToLocalTime()
                });

            return Json(lista);
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
