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
        private readonly AppDbContext _context;

        public EstoqueController(IEstoqueService estoqueService, AppDbContext context)
        {
            _estoqueService = estoqueService;
            _context = context;
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
            var query = _context.Estoques
               
                .Include(e => e.Fornecedor)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.Nome.Contains(search) || e.Codigo.Contains(search));
            }

            if (categoriaId.HasValue)
            {
              //  query = query.Where(e => e.Categorias == categoriaId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            var estoques = await query
                .OrderBy(e => e.Nome)
                .ToListAsync();

            // Carregar dados para os dropdowns
            ViewBag.Categorias = await _context.CategoriasEstoque
                .Where(c => c.Status == StatusEstoque.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();

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
                Categorias = await _context.Estoques
                    .Select(e => e.Categorias)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),

                Fornecedores = await _context.Fornecedores.Where(f => f.Status == StatusFornecedor.Ativo).ToListAsync(),
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
            model.Fornecedores = await _context.Fornecedores.Where(f => f.Status ==  StatusFornecedor.Ativo).ToListAsync();

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
                UnidadeMedida = estoque.UnidadeMedida,
                Status = estoque.Status,
               // CategoriaEstoqueId = estoque.CategoriaEstoqueId,
                FornecedorId = estoque.FornecedorId,
                Categorias = await _context.Estoques
                    .Select(e => e.Categorias)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),
                Fornecedores = await _context.Fornecedores.Where(f => f.Status == StatusFornecedor.Ativo).ToListAsync()
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
            model.Fornecedores = await _context.Fornecedores.Where(f => f.Status == StatusFornecedor.Ativo).ToListAsync();

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
            ViewBag.Produtos = await _context.Estoques
                .Where(e => e.Status == StatusEstoque.Ativo)
                .OrderBy(e => e.Nome)
                .ToListAsync();

            var movimentacoes = await _estoqueService.ObterMovimentacoesAsync(dataInicio, dataFim, produtoId);
            return View(movimentacoes);
        }

        // GET: Estoque/NovaMovimentacao
        // Controllers/EstoqueController.cs - Adicione este método
        public async Task<IActionResult> NovaMovimentacao(int? produtoId)
        {
            var model = new MovimentacaoEstoqueViewModel
            {
                Produtos = await _context.Estoques
                    .Where(e => e.Status == StatusEstoque.Ativo)
                    .OrderBy(e => e.Nome)
                    .ToListAsync(),
                Fornecedores = await _context.Fornecedores
                    .Where(f => f.Status == StatusFornecedor.Ativo)
                    .OrderBy(f => f.Nome)
                    .ToListAsync(),
                Pedidos = await _context.Pedidos
                    .Where(p => p.Status == PedidoStatus.Finalizado)
                    .OrderByDescending(p => p.DataAbertura)
                    .Take(50)
                    .ToListAsync()
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
                    model.Produtos = await _context.Estoques
                        .Where(e => e.Status == StatusEstoque.Ativo)
                        .OrderBy(e => e.Nome)
                        .ToListAsync();
                    model.Fornecedores = await _context.Fornecedores
                        .Where(f => f.Status == StatusFornecedor.Ativo)
                        .OrderBy(f => f.Nome)
                        .ToListAsync();
                    model.Pedidos = await _context.Pedidos
                        .Where(p => p.Status == PedidoStatus.Finalizado)
                        .OrderByDescending(p => p.DataAbertura)
                        .Take(50)
                        .ToListAsync();
                }
            }
            else
            {
                // Recarregar dropdowns se ModelState for inválido
                model.Produtos = await _context.Estoques
                    .Where(e => e.Status == StatusEstoque.Ativo)
                    .OrderBy(e => e.Nome)
                    .ToListAsync();
                model.Fornecedores = await _context.Fornecedores
                    .Where(f => f.Status == StatusFornecedor.Ativo)
                    .OrderBy(f => f.Nome)
                    .ToListAsync();
                model.Pedidos = await _context.Pedidos
                    .Where(p => p.Status == PedidoStatus.Finalizado)
                    .OrderByDescending(p => p.DataAbertura)
                    .Take(50)
                    .ToListAsync();
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
            var query = _context.Inventarios
                .Include(i => i.Itens)
                .ThenInclude(ii => ii.Estoque)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var inventarios = await query
                .OrderByDescending(i => i.DataAbertura)
                .ToListAsync();

            return View(inventarios);
        }

        public async Task<IActionResult> DetalhesInventario(int id)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Itens)
                .ThenInclude(ii => ii.Estoque)
                .FirstOrDefaultAsync(i => i.Id == id);

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
                var inventario = new Inventario
                {
                    Descricao = descricao,
                    Observacao = observacao,
                    Status = StatusInventario.Aberto,
                    DataAbertura = DateTime.Now
                };

                _context.Inventarios.Add(inventario);
                await _context.SaveChangesAsync();

                // Adicionar itens ao inventário
                if (incluirTodosProdutos)
                {
                    var produtosAtivos = await _context.Estoques
                        .Where(e => e.Status == StatusEstoque.Ativo)
                        .ToListAsync();

                    foreach (var produto in produtosAtivos)
                    {
                        var item = new ItemInventario
                        {
                            InventarioId = inventario.Id,
                            EstoqueId = produto.Id,
                            QuantidadeSistema = produto.EstoqueAtual,
                            QuantidadeFisica = 0,
                            CustoUnitario = produto.CustoUnitario,
                            Conferido = false
                        };

                        _context.ItensInventario.Add(item);
                    }

                    await _context.SaveChangesAsync();
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
            var inventario = await _context.Inventarios
                .Include(i => i.Itens)
                .ThenInclude(ii => ii.Estoque)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null || inventario.Status == StatusInventario.Concluido)
            {
                return NotFound();
            }

            return View(inventario);
        }

        [HttpPost]
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