// Controllers/EstoqueController.cs
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.EstoquesModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Fynanceo.Controllers
{
    public class EstoqueController : Controller
    {
        private readonly IEstoqueService _estoqueService;
        private readonly IFornecedorService _fornecedorService;
        private readonly IPedidoService _pedidoService;
       

        public EstoqueController(IEstoqueService estoqueService,  IFornecedorService fornecedorService, IPedidoService pedidoService)
        {
            _estoqueService = estoqueService;
      
            _fornecedorService = fornecedorService;
            _pedidoService = pedidoService;
        }

        // GET: Estoque/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _estoqueService.ObterDadosDashboardAsync();
            return View(dashboard);
        }

        // GET: Estoque/Index
        public async Task<IActionResult> Index(string search, int? categoriaId, StatusEstoque? status)
        {
            // Carregar todos os estoques pela service (lista em memória)
            var estoquesLista = await _estoqueService.ObterTodosEstoquesAsync();

            // Aplicar filtros em memória
            if (!string.IsNullOrEmpty(search))
            {
                estoquesLista = estoquesLista
                    .Where(e => (e.Nome != null && e.Nome.Contains(search)) || (e.Codigo != null && e.Codigo.Contains(search)))
                    .ToList();
            }

     

            if (status.HasValue)
            {
                estoquesLista = estoquesLista
                    .Where(e => e.Status == status.Value)
                    .ToList();
            }

            var estoques = estoquesLista
                .OrderBy(e => e.Nome)
                .ToList();

            // Carregar dados para os dropdowns
            ViewBag.Categorias = await _estoqueService.SomenteCategoriasAsync();
            
            // Calcular contador de alertas
            var produtosAlerta = await _estoqueService.ObterProdutosEstoqueMinimoAsync();
            ViewBag.AlertCount = produtosAlerta.Count;
            
            return View(estoques);
         
        }

        // GET: Estoque/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var estoque = await _estoqueService.ObterEstoquePorIdAsync(id);
            if (estoque == null)
            {
                return NotFound();
            }
            var vm = new DetailsProdutoViewModel
            {
                Id = estoque.Id,
                Nome = estoque.Nome,
                Categoria = estoque.Categorias,
                Descricao = estoque.Descricao,
                CustoUnitario = estoque.CustoUnitario,
          EstoqueMaximo = estoque.EstoqueMaximo,
          EstoqueMinimo = estoque.EstoqueMinimo,
                EstoqueAtual = estoque.EstoqueAtual,
                DataCadastro = estoque.DataCriacao,
                NomeFornecedor = estoque.Fornecedor.Nome,
                Codigo = estoque.Codigo,
                
                
            };

            return View(vm);
        }

        // GET: Estoque/Create
        public async Task<IActionResult> Create()
        {
            
            var model = new EstoqueViewModel
            {
                Categorias = await _estoqueService.SomenteCategoriasAsync(),

                Fornecedores = await _fornecedorService.ObterFornecedoresAtivosAsync(),
                Status = StatusEstoque.Ativo
            };

            return View(model);
        }

        // POST: Estoque/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EstoqueViewModel model)
        {
           
            {
                try
                {
                    await _estoqueService.CriarEstoqueAsync(model);
                    TempData["Success"] = "Produto cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao cadastrar produto: {ex.Message}");
                }
            }

            // Recarregar dropdowns em caso de erro
          //  model.Categorias = await _context.CategoriasEstoque.Where(c => c.Status == StatusEstoque.Ativo).ToListAsync();
            model.Fornecedores = await _fornecedorService.ObterFornecedoresAtivosAsync();

            return View(model);
        }

        // GET: Estoque/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var estoque = await _estoqueService.ObterEstoquePorIdAsync(id);
            if (estoque == null)
            {
                return NotFound();
            }

            var model = new EstoqueViewModel
            {
                Id = estoque.Id,
                Nome = estoque.Nome,
                Codigo = estoque.Codigo,
                Descricao = estoque.Descricao,
                EstoqueAtual = estoque.EstoqueAtual,
                EstoqueMinimo = estoque.EstoqueMinimo,
                EstoqueMaximo = estoque.EstoqueMaximo,
                CustoUnitario = estoque.CustoUnitario,
                StatusUnidadeMedida = estoque.UnidadeMedida,
                Status = estoque.Status,
               // CategoriaEstoqueId = estoque.CategoriaEstoqueId,
                FornecedorId = estoque.FornecedorId,
                Categorias = await _estoqueService.SomenteCategoriasAsync(),
                Fornecedores = await _fornecedorService.ObterFornecedoresAtivosAsync(),
            };

            return View(model);
        }

        // POST: Estoque/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EstoqueViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _estoqueService.AtualizarEstoqueAsync(id, model);
                    TempData["Success"] = "Produto atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao atualizar produto: {ex.Message}");
                }
            }

            // Recarregar dropdowns em caso de erro
        //   model.Categorias = await _context.CategoriasEstoque.Where(c => c.Status == StatusEstoque.Ativo).ToListAsync();
         model.Fornecedores = await _fornecedorService.ObterFornecedoresAtivosAsync();

            return View(model);
        }

        // GET: Estoque/Movimentacoes
    
        public async Task<IActionResult> Movimentacoes(DateTime? dataInicio, DateTime? dataFim, int? produtoId)
        {
            dataInicio ??= DateTime.Today.AddDays(-30);
            dataFim ??= DateTime.Today;

            ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
            ViewBag.ProdutoId = produtoId;

            // Carregar lista de produtos para o dropdown
            ViewBag.Produtos = await _estoqueService.ObterTodosEstoquesAsync();

            var movimentacoes = await _estoqueService.ObterMovimentacoesAsync(dataInicio, dataFim, produtoId);
            return View(movimentacoes);
        }

        // GET: Estoque/NovaMovimentacao
     
        public async Task<IActionResult> NovaMovimentacao(int? produtoId)
        {
            var model = new MovimentacaoEstoqueViewModel
            {
                Produtos = await _estoqueService.ObterTodosEstoquesAsync(),
                Fornecedores = await _fornecedorService.ObterFornecedoresAtivosAsync(),
                
                Pedidos = await _pedidoService.ObterPedidosPorStatus( "Finalizado"),
                
            };

            // Pre-selecionar produto se veio por parâmetro
            if (produtoId.HasValue)
            {
                model.EstoqueId = produtoId.Value;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovaMovimentacao(MovimentacaoEstoqueViewModel model)
        {
            model.UsuarioNome = "usuario";
           
            if (ModelState.IsValid)
            {
                try
                {
                    var movimentacao = await _estoqueService.CriarMovimentacaoAsync(model);
                    TempData["Success"] = $"Movimentação de {model.Tipo} registrada com sucesso!";
                    return RedirectToAction(nameof(Movimentacoes));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao registrar movimentação: {ex.Message}");

                    // Recarregar dropdowns em caso de erro
                    model.Produtos = await _estoqueService.ObterTodosEstoquesAsync();
                    model.Fornecedores = await _fornecedorService.ObterFornecedoresPorStatusAsync(StatusFornecedor.Ativo);
                    model.Pedidos = await _pedidoService.ObterPedidosPorStatus("Finalizado");
                }
            }
            else
            {
                // Recarregar dropdowns se ModelState for inválido
                model.Produtos = await _estoqueService.ObterTodosEstoquesAsync();
                model.Fornecedores = await _fornecedorService.ObterFornecedoresPorStatusAsync(StatusFornecedor.Ativo);
                model.Pedidos = await _pedidoService.ObterPedidosPorStatus("Finalizado");
            }

            return View(model);
        }

        // GET: Estoque/Alertas
        public async Task<IActionResult> Alertas()
        {
            var viewModel = new
            {
                EstoqueMinimo = await _estoqueService.ObterProdutosEstoqueMinimoAsync(),
                EstoqueZero = await _estoqueService.ObterProdutosEstoqueZeroAsync()
            };

            return View(viewModel);
        }

        // GET: Estoque/Inventario
        //public async Task<IActionResult> Inventario()
        //{
        //    var inventarios = await _estoqueService.ObterInventariosAsync();
        //    return View(inventarios);
        //}
        // Controllers/EstoqueController.cs - Adicione estes métodos
        public async Task<IActionResult> Inventario(StatusInventario? status = null)
        {
            var inventarios = await _estoqueService.ObterInventariosAsync();

            if (status.HasValue)
            {
                inventarios = inventarios
                    .Where(i => i.Status == status.Value)
                    .ToList();
            }

            inventarios = inventarios
                .OrderByDescending(i => i.DataAbertura)
                .ToList();

            return View(inventarios);
        }

        public async Task<IActionResult> DetalhesInventario(int id)
        {
            var inventario = await _estoqueService.ObterInventarioPorIdAsync(id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
            
            
            
        }

        [HttpPost]
        public async Task<IActionResult> CriarInventario(string descricao, string observacao, bool incluirTodosProdutos = true)
        {
            try
            {
                // Criar inventário via service
                var model = new InventarioViewModel
                {
                    Descricao = descricao,
                    Observacao = observacao
                };

                var inventario = await _estoqueService.CriarInventarioAsync(model);

                // Adicionar itens via service (mantendo conferido=false por padrão)
                if (incluirTodosProdutos)
                {
                    await _estoqueService.AdicionarItensInventarioTodosAsync(inventario.Id, apenasAtivos: true, conferido: false);
                }

                TempData["Success"] = $"Inventário '{descricao}' criado com sucesso!";
                return RedirectToAction(nameof(EditarInventario), new { id = inventario.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao criar inventário: {ex.Message}";
                return RedirectToAction(nameof(Inventario));
            }
        }

        public async Task<IActionResult> EditarInventario(int id)
        {
            var inventario = await _estoqueService.ObterInventarioPorIdAsync(id);

            if (inventario == null || inventario.Status == StatusInventario.Concluido)
            {
                return NotFound();
            }

            return View(inventario);
        }
        // Controllers/EstoqueController.cs - Conferir item via Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConferirItem(int itemId, int inventarioId, decimal quantidadeFisica, string observacao)
        {
            try
            {
                var result = await _estoqueService.ConferirItemAsync(itemId, inventarioId, quantidadeFisica, observacao);
                return Json(new { success = result.success, message = result.message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FecharInventario(int id)
        {
            try
            {
                var result = await _estoqueService.FecharInventarioAsync(id);
                if (result != null)
                {
                    TempData["Success"] = "Inventário fechado com sucesso! Ajustes de estoque foram aplicados.";
                }
                else
                {
                    TempData["Error"] = "Erro ao fechar inventário.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao fechar inventário: {ex.Message}";
            }

            return RedirectToAction(nameof(Inventario));
        }
    }
}