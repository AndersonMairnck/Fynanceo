// Controllers/CaixaController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.CaixaModel;
using Fynanceo.ViewModel.FinanceirosModel;

namespace Fynanceo.Controllers
{
    public class CaixaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFinanceiroService _financeiroService;
       
        public CaixaController(AppDbContext context, IFinanceiroService financeiroService )
        {
            _context = context;
            _financeiroService = financeiroService;
          
        }

        // GET: Caixa/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _financeiroService.ObterDashboardFinanceiro();
            return View(dashboard);
        }

        // GET: Caixa/Index
        public async Task<IActionResult> Index()
        {
            var caixas = await _financeiroService.ObterCaixasPeriodo(
                DateTime.Today.AddDays(-30),
                DateTime.Today
            );
            return View(caixas);
        }

        // GET: Caixa/Details/5
        public async Task<IActionResult> Details(int id)
        {

            var caixa = await _financeiroService.ObterCaixaPorId(id);
            // var caixa = await _context.Caixas
            //     .Include(c => c.Movimentacoes)
            //     .FirstOrDefaultAsync(c => c.Id == id);

            if (caixa == null)
            {
                return NotFound();
            }

            return View(caixa);
        }

        // GET: Caixa/Abrir
        public IActionResult Abrir()
        {
            var caixaAberto = _financeiroService.ObterCaixaAberto().Result;
            if (caixaAberto != null)
            {
                TempData["Warning"] = "Já existe um caixa aberto!";
                return RedirectToAction(nameof(Details), new { id = caixaAberto.Id });
            }

            return View(new CaixaViewModel());
        }

        // POST: Caixa/Abrir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(CaixaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var caixa = await _financeiroService.AbrirCaixa(viewModel);
                    TempData["Success"] = "Caixa aberto com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = caixa.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao abrir caixa: {ex.Message}");
                }
            }
            return View(viewModel);
        }

        // GET: Caixa/Fechar/5
        public async Task<IActionResult> Fechar(int id)
        {
            var caixa = await _context.Caixas
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => c.Id == id && !c.Fechado);

            if (caixa == null)
            {
                TempData["Error"] = "Caixa não encontrado ou já fechado";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FechamentoCaixaViewModel()  
            {
                CaixaId = caixa.Id,
                SaldoFinal = caixa.SaldoFinal
            };

            return View(viewModel);
        }

        // POST: Caixa/Fechar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fechar(FechamentoCaixaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var caixa = await _financeiroService.FecharCaixa(viewModel);
                    TempData["Success"] = "Caixa fechado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = caixa.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao fechar caixa: {ex.Message}");
                }
            }
            return View(viewModel);
        }

        // POST: Caixa/AdicionarMovimentacao
        [HttpPost]
        public async Task<JsonResult> AdicionarMovimentacao(MovimentacaoViewModel viewModel)
        {
            try
            {
                var movimentacao = await _financeiroService.AdicionarMovimentacao(viewModel);
                return Json(new { success = true, message = "Movimentação adicionada com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Caixa/Movimentacoes/5
        public async Task<JsonResult> Movimentacoes(int id)
        {
            var movimentacoes = await _financeiroService.ObterMovimentacoesCaixa(id);
            var result = movimentacoes.Select(m => new
            {
                id = m.Id,
                tipo = m.Tipo.ToString(),
                valor = m.Valor,
                formaPagamento = m.FormaPagamento.ToString(),
                categoria = m.Categoria.ToString(),
                descricao = m.Descricao,
                data = m.DataMovimentacao.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                observacoes = m.Observacoes
            }).ToList();

            return Json(result);
        }
    }
}