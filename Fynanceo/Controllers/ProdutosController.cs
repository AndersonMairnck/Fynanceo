using Microsoft.AspNetCore.Mvc;
using Fynanceo.Services;
using Fynanceo.ViewModels;

namespace Fynanceo.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        public async Task<IActionResult> Index()
        {
            var produtos = await _produtoService.ObterTodosAsync();
            return View(produtos);
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            if (produto == null)
            {
                return NotFound();
            }
            return View(produto);
        }

        public IActionResult Cadastrar()
        {
            var model = new ProdutoViewModel();
            // Adicionar um ingrediente vazio para o template
            model.Ingredientes.Add(new IngredienteViewModel());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(ProdutoViewModel model)
        {
            try { 
            if (ModelState.IsValid)
            {
                if (await _produtoService.CodigoExisteAsync(model.Codigo))
                {
                    ModelState.AddModelError("Codigo", "Código já cadastrado.");
                    return View(model);
                }

                if (model.ValorVenda <= model.CustoUnitario)
                {
                    ModelState.AddModelError("ValorVenda", "Valor de venda deve ser maior que o custo unitário.");
                    return View(model);
                }

                var resultado = await _produtoService.AdicionarAsync(model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Produto cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao cadastrar produto. Tente novamente.");
            }


                // Se houver erros, capturar campos inválidos
                if (!ModelState.IsValid)
                {
                    var camposInvalidos = ModelState
                        .Where(ms => ms.Value.Errors.Any())
                        .Select(ms => new { Campo = ms.Key, Erros = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                        .ToList();

                    // Opcional: você pode enviar para a view via ViewBag
                    ViewBag.CamposInvalidos = camposInvalidos;
                }


                // Garantir que há pelo menos um ingrediente para o template
                if (model.Ingredientes.Count == 0)
                model.Ingredientes.Add(new IngredienteViewModel());

            return View(model);
                }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao cadastrar produto: {ex.Message}");
                return View(model);
            }
        }

        public async Task<IActionResult> Editar(int id)
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            var model = new ProdutoViewModel
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Categoria = produto.Categoria,
                Subcategoria = produto.Subcategoria,
                CustoUnitario = produto.CustoUnitario,
                ValorVenda = produto.ValorVenda,
                TempoPreparoMinutos = produto.TempoPreparoMinutos,
                TempoExtraPico = produto.TempoExtraPico,
                OpcoesPersonalizacao = produto.OpcoesPersonalizacao,
                Disponivel = produto.Disponivel,
              ////  MotivoIndisponibilidade = produto.MotivoIndisponibilidade,
              //  CodigoNCM = produto.CodigoNCM,
              //  Origem = produto.Origem,
              //  CST = produto.CST,
              //  Aliquota = produto.Aliquota
            };

            // Converter ingredientes para ViewModel
            foreach (var ingrediente in produto.Ingredientes)
            {
                model.Ingredientes.Add(new IngredienteViewModel
                {
                    Id = ingrediente.Id,
                    Nome = ingrediente.Nome,
                    Quantidade = ingrediente.Quantidade,
                    UnidadeMedida = ingrediente.UnidadeMedida
                });
            }

            // Garantir que há pelo menos um ingrediente para o template
            if (model.Ingredientes.Count == 0)
                model.Ingredientes.Add(new IngredienteViewModel());

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ProdutoViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _produtoService.CodigoExisteAsync(model.Codigo, id))
                {
                    ModelState.AddModelError("Codigo", "Código já cadastrado.");
                    return View(model);
                }

                if (model.ValorVenda <= model.CustoUnitario)
                {
                    ModelState.AddModelError("ValorVenda", "Valor de venda deve ser maior que o custo unitário.");
                    return View(model);
                }

                var resultado = await _produtoService.AtualizarAsync(id, model);
                if (resultado)
                {
                    TempData["Sucesso"] = "Produto atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Erro ao atualizar produto. Tente novamente.");
            }

            // Garantir que há pelo menos um ingrediente para o template
            if (model.Ingredientes.Count == 0)
                model.Ingredientes.Add(new IngredienteViewModel());

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var resultado = await _produtoService.ExcluirAsync(id);
            if (resultado)
            {
                TempData["Sucesso"] = "Produto excluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir produto. Tente novamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AdicionarIngrediente([FromBody] ProdutoViewModel model)
        {
            model.Ingredientes.Add(new IngredienteViewModel());
            return PartialView("_IngredientesPartial", model);
        }
    }
}