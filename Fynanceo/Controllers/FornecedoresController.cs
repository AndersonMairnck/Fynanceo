// Controllers/FornecedoresController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;

namespace Fynanceo.Controllers
{
    public class FornecedoresController : Controller
    {
        private readonly AppDbContext _context;

        public FornecedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Fornecedores
        public async Task<IActionResult> Index()
        {
            var fornecedores = await _context.Fornecedores
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return View(fornecedores);
        }

        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var fornecedor = await _context.Fornecedores
                .Include(f => f.Contas)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        // GET: Fornecedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fornecedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fornecedor);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fornecedor cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(fornecedor);
        }

        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null)
            {
                return NotFound();
            }
            return View(fornecedor);
        }

        // POST: Fornecedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Fornecedor fornecedor)
        {
            if (id != fornecedor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fornecedor);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fornecedor atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornecedorExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fornecedor);
        }

        // POST: Fornecedores/Delete/5
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null)
            {
                return Json(new { success = false, message = "Fornecedor não encontrado" });
            }

            // Verificar se tem contas vinculadas
            var temContas = await _context.Contas.AnyAsync(c => c.FornecedorId == id);
            if (temContas)
            {
                return Json(new { success = false, message = "Não é possível excluir fornecedor com contas vinculadas" });
            }

            _context.Fornecedores.Remove(fornecedor);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Fornecedor excluído com sucesso" });
        }

        private bool FornecedorExists(int id)
        {
            return _context.Fornecedores.Any(e => e.Id == id);
        }
    }
}