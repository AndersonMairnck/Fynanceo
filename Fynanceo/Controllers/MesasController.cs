using Microsoft.AspNetCore.Mvc;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.MesasModel;
using Microsoft.AspNetCore.Authorization;

namespace Fynanceo.Controllers
{
    [Authorize (Roles = "Administrador, Gerente, Atendente")]
    public class MesasController : Controller
    {
        private readonly IMesaService _mesaService;

        public MesasController(IMesaService mesaService)
        {
            _mesaService = mesaService;
        }

        public async Task<IActionResult> Index()
        {
            var mesas = await _mesaService.ObterTodosAsync();
            return View(mesas);
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var mesa = await _mesaService.ObterPorIdAsync(id);
            if (mesa == null)
            {
                return NotFound();
            }
            return View(mesa);
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(MesaViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _mesaService.NumeroExisteAsync(model.Numero))
                {
                    ModelState.AddModelError("Numero", "Número da mesa já cadastrado.");
                    return View(model);
                }

                var resultado = await _mesaService.AdicionarAsync(model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Mesa cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao cadastrar mesa. Tente novamente.");
            }
            return View(model);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var mesa = await _mesaService.ObterPorIdAsync(id);
            if (mesa == null)
            {
                return NotFound();
            }

            var model = new MesaViewModel
            {
                Id = mesa.Id,
                Numero = mesa.Numero,
                Capacidade = mesa.Capacidade,
                Localizacao = mesa.Localizacao,
                Ambiente = mesa.Ambiente,
                Status = mesa.Status,
                Descricao = mesa.Descricao
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, MesaViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _mesaService.NumeroExisteAsync(model.Numero, id))
                {
                    ModelState.AddModelError("Numero", "Número da mesa já cadastrado.");
                    return View(model);
                }

                var resultado = await _mesaService.AtualizarAsync(id, model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Mesa atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao atualizar mesa. Tente novamente.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var resultado = await _mesaService.ExcluirAsync(id);
            if (resultado)
            {
                TempData["Sucesso"] = "Mesa excluída com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir mesa. Tente novamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarStatus(int id, string status)
        {
            var resultado = await _mesaService.AtualizarStatusAsync(id, status);
            if (resultado)
            {
                TempData["Sucesso"] = "Status da mesa atualizado com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao atualizar status da mesa.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}