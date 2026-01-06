using Fynanceo.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.ProdutosModel;
using Microsoft.AspNetCore.Authorization;

namespace Fynanceo.Controllers
{
    [Authorize(Roles = "Administrador,Gerente")]
    public class ProdutosController : Controller
    {
        private readonly IProdutoService _produtoService;
        private readonly IEstoqueService _estoqueService;
        private object _logger;

        public ProdutosController(IProdutoService produtoService, IEstoqueService estoqueService)
        {
            _produtoService = produtoService;
            _estoqueService = estoqueService;
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

        public async Task<IActionResult> Cadastrar()
        {
            return View(new ProdutoViewModel
            {
                Ingredientes = (await _estoqueService.ObterTodosEstoquesAsync())
                    .Select(e => new MateriaisProdutoViewModel
                    {
                        TipoItem = e.TipoItem,
                        IdEstoque = e.Id,
                        Nome = e.Nome,
                        Quantidade = 0,
                        UnidadeMedida = (StatusUnidadeMedida)e.UnidadeMedida,
                     
                        Codigo = e.Codigo,
                    }).ToList()
            });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastrar(ProdutoViewModel model)
        {
          
            // Ignorar ingredientes vazios
            if (model.Ingredientes != null)
            {
                foreach (var ing in model.Ingredientes)
                {
                    bool vazio =
                        string.IsNullOrWhiteSpace(ing.Nome) &&
                        ing.Quantidade == 0;
                  
                    if (vazio)
                    {
                        int index = model.Ingredientes.IndexOf(ing);

                        ModelState.Remove($"Ingredientes[{index}].Nome");
                        ModelState.Remove($"Ingredientes[{index}].Quantidade");
                        ModelState.Remove($"Ingredientes[{index}].UnidadeMedida");
                    }
                }
            }

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
                model.Ingredientes.Add(new MateriaisProdutoViewModel());

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
                
              
            };

            // Converter ingredientes para ViewModel
            foreach (var ingrediente in produto.ProdutoIngredientes)
            {
                model.Ingredientes.Add(new MateriaisProdutoViewModel
                {
                    IdEstoque = ingrediente.EstoqueId,
                    Nome = ingrediente.Estoque.Nome,
                    Quantidade = ingrediente.Quantidade,
                   UnidadeMedida = ingrediente.UnidadeMedida,
                   Observacao = ingrediente.Observacao
                });
            }

            // Garantir que há pelo menos um ingrediente para o template
            if (model.Ingredientes.Count == 0)
                model.Ingredientes.Add(new MateriaisProdutoViewModel());

            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMateriaisEstoque(int page = 1, int pageSize = 20, string search = "", string categoria = "")
        {
            try
            {
                var estoques = (await _estoqueService.ObterTodosEstoquesAsync()).ToList();
        
                // Aplicar filtro de busca
                if (!string.IsNullOrEmpty(search))
                {
                    estoques = estoques.Where(e => 
                            (e.Nome?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (e.Codigo?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                        .ToList();
                }
        
                // Aplicar filtro de categoria
                if (!string.IsNullOrEmpty(categoria))
                {
                    estoques = estoques.Where(e => 
                            (e.Categorias?.Contains(categoria, StringComparison.OrdinalIgnoreCase) ?? false))
                        .ToList();
                }
        
                // Ordenar por TipoItem (N primeiro, depois R, depois demais) e então por nome
                estoques = estoques
                    .OrderBy(e => (e.TipoItem ?? "") == "N" ? 0 : (e.TipoItem ?? "") == "R" ? 1 : 2)
                    .ThenBy(e => e.Nome)
                    .ToList();
        
                // Paginação
                var totalCount = estoques.Count;
                var materiais = estoques
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        id = e.Id,
                        nome = e.Nome,
                        tipoItem = e.TipoItem,
                        codigo = e.Codigo,
                        unidadeMedida = e.UnidadeMedida.ToString(),
                        quantidadeDisponivel = e.EstoqueAtual,
                        categoria = e.Categorias
                    })
                    .ToList();
        
                return Json(new
                {
                    materiais,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    currentPage = page,
                    totalCount
                });
            }
            catch (Exception ex)
            {
               //_logger.LogError(ex, "Erro ao carregar materiais do estoque");
                return StatusCode(500, new { error = "Erro ao carregar materiais do estoque" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ProdutoViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            // Ignorar ingredientes vazios
            if (model.Ingredientes != null)
            {
                foreach (var ing in model.Ingredientes)
                {
                    bool vazio =
                        string.IsNullOrWhiteSpace(ing.Nome) &&
                        ing.Quantidade == 0;
                        //&&
                      //  string.IsNullOrWhiteSpace(ing.UnidadeMedida);

                    if (vazio)
                    {
                        int index = model.Ingredientes.IndexOf(ing);

                        ModelState.Remove($"Ingredientes[{index}].Nome");
                        ModelState.Remove($"Ingredientes[{index}].Quantidade");
                        ModelState.Remove($"Ingredientes[{index}].UnidadeMedida");
                    }
                }
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
                model.Ingredientes.Add(new MateriaisProdutoViewModel());

            return View(model);
        }

        [HttpPost]
     //   [ValidateAntiForgeryToken]
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
            model.Ingredientes.Add(new MateriaisProdutoViewModel());
            return PartialView("_IngredientesPartial", model);
        }
    }
}