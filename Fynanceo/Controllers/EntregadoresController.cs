// Controllers/EntregadoresController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.ViewModels;
using Fynanceo.Services;

namespace Fynanceo.Controllers
{
    public class EntregadoresController : Controller
    {
        private readonly AppDbContext _context;

        public EntregadoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Entregadores
        public async Task<IActionResult> Index()
        {
            var entregadores = await _context.Entregadores
                .OrderBy(e => e.Nome)
                .ToListAsync();

            return View(entregadores);
        }

        // GET: Entregadores/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entregador = await _context.Entregadores
                .Include(e => e.Entregas)
                    .ThenInclude(ent => ent.Pedido)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entregador == null)
            {
                return NotFound();
            }

            return View(entregador);
        }

        // GET: Entregadores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Entregadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EntregadorViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var entregador = new Entregador
                {
                    Nome = viewModel.Nome,
                    Telefone = viewModel.Telefone,
                    TipoVeiculo = viewModel.TipoVeiculo,
                    Placa = viewModel.Placa,
                    ModeloVeiculo = viewModel.ModeloVeiculo,
                    CorVeiculo = viewModel.CorVeiculo,
                    Observacoes = viewModel.Observacoes,
                    Ativo = viewModel.Ativo,
                    Status = Fynanceo.Models.Enums.StatusEntregador.Disponivel
                };

                _context.Add(entregador);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Entregador cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Entregadores/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entregador = await _context.Entregadores.FindAsync(id);
            if (entregador == null)
            {
                return NotFound();
            }

            var viewModel = new EntregadorViewModel
            {
                Id = entregador.Id,
                Nome = entregador.Nome,
                Telefone = entregador.Telefone,
                TipoVeiculo = entregador.TipoVeiculo,
                Placa = entregador.Placa,
                ModeloVeiculo = entregador.ModeloVeiculo,
                CorVeiculo = entregador.CorVeiculo,
                Observacoes = entregador.Observacoes,
                Ativo = entregador.Ativo
            };

            return View(viewModel);
        }

        // POST: Entregadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EntregadorViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var entregador = await _context.Entregadores.FindAsync(id);
                if (entregador == null)
                {
                    return NotFound();
                }

                entregador.Nome = viewModel.Nome;
                entregador.Telefone = viewModel.Telefone;
                entregador.TipoVeiculo = viewModel.TipoVeiculo;
                entregador.Placa = viewModel.Placa;
                entregador.ModeloVeiculo = viewModel.ModeloVeiculo;
                entregador.CorVeiculo = viewModel.CorVeiculo;
                entregador.Observacoes = viewModel.Observacoes;
                entregador.Ativo = viewModel.Ativo;

                try
                {
                    _context.Update(entregador);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Entregador atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntregadorExists(id))
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
            return View(viewModel);
        }

        // POST: Entregadores/Delete/5
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var entregador = await _context.Entregadores.FindAsync(id);
            if (entregador == null)
            {
                return Json(new { success = false, message = "Entregador não encontrado" });
            }

            // Verificar se tem entregas em andamento
            var entregasAtivas = await _context.Entregas
                .AnyAsync(e => e.EntregadorId == id &&
                              (e.Status == Fynanceo.Models.Enums.StatusEntrega.SaiuParaEntrega ||
                               e.Status == Fynanceo.Models.Enums.StatusEntrega.EmRota));

            if (entregasAtivas)
            {
                return Json(new { success = false, message = "Não é possível excluir entregador com entregas em andamento" });
            }

            _context.Entregadores.Remove(entregador);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Entregador excluído com sucesso" });
        }

        // POST: Entregadores/AtualizarStatus/5
        [HttpPost]
        public async Task<JsonResult> AtualizarStatus(int id, string novoStatus)
        {
            var entregador = await _context.Entregadores.FindAsync(id);
            if (entregador == null)
            {
                return Json(new { success = false, message = "Entregador não encontrado" });
            }

            if (Enum.TryParse<Fynanceo.Models.Enums.StatusEntregador>(novoStatus, out var status))
            {
                entregador.Status = status;
                entregador.UltimaAtualizacao = DateTime.Now;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Status atualizado com sucesso" });
            }

            return Json(new { success = false, message = "Status inválido" });
        }

        private bool EntregadorExists(int id)
        {
            return _context.Entregadores.Any(e => e.Id == id);
        }
    }
}