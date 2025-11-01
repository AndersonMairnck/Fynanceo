// Controllers/ContasController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.ContasModel;

namespace Fynanceo.Controllers
{
    public class ContasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFinanceiroService _financeiroService;

        public ContasController(AppDbContext context, IFinanceiroService financeiroService)
        {
            _context = context;
            _financeiroService = financeiroService;
        }

        // GET: Contas
        public async Task<IActionResult> Index()
        {
            var contas = await _context.Contas
                .Include(c => c.Fornecedor)
                .OrderByDescending(c => c.DataVencimento)
                .ToListAsync();

            return View(contas);
        }

        // GET: Contas/Pendentes
        public async Task<IActionResult> Pendentes()
        {
            var contas = await _financeiroService.ObterContasPendentes();
            return View(contas);
        }

        // GET: Contas/Atrasadas
        public async Task<IActionResult> Atrasadas()
        {
            var contas = await _financeiroService.ObterContasAtrasadas();
            return View(contas);
        }

        // GET: Contas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var conta = await _context.Contas
                .Include(c => c.Fornecedor)
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null)
            {
                return NotFound();
            }

            return View(conta);
        }

        // GET: Contas/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ContaViewModel
            {
                Fornecedores = await _context.Fornecedores
                    .Where(f => f.Ativo)
                    .OrderBy(f => f.Nome)
                    .ToListAsync(),
                DataVencimento = DateTime.Today.AddDays(7)
            };

            return View(viewModel);
        }

        // POST: Contas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var conta = await _financeiroService.CriarConta(viewModel);
                    TempData["Success"] = "Conta criada com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = conta.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao criar conta: {ex.Message}");
                }
            }

            // Recarregar fornecedores se houver erro
            viewModel.Fornecedores = await _context.Fornecedores
                .Where(f => f.Ativo)
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: Contas/Pagar/5
        public async Task<IActionResult> Pagar(int id)
        {
            var conta = await _context.Contas
                .Include(c => c.Fornecedor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null || conta.Status != StatusConta.Pendente)
            {
                TempData["Error"] = "Conta não encontrada ou já paga";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PagamentoContaViewModel
            {
                ContaId = conta.Id,
                ValorPago = conta.Valor,
                DataPagamento = DateTime.Today
            };

            return View(viewModel);
        }

        // POST: Contas/Pagar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagar(PagamentoContaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var conta = await _financeiroService.PagarConta(viewModel);
                    TempData["Success"] = "Conta paga com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = conta.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao pagar conta: {ex.Message}");
                }
            }
            return View(viewModel);
        }

        // POST: Contas/Cancelar/5
        [HttpPost]
        public async Task<JsonResult> Cancelar(int id)
        {
            try
            {
                var conta = await _context.Contas.FindAsync(id);
                if (conta == null)
                    return Json(new { success = false, message = "Conta não encontrada" });

                if (conta.Status != StatusConta.Pendente)
                    return Json(new { success = false, message = "Conta não está pendente" });

                conta.Status = StatusConta.Cancelada;
                conta.DataAlteracao = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Conta cancelada com sucesso" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}