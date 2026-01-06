// Controllers/EstoqueController.cs

using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.EstoquesModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using Fynanceo.Configuracao;
using Microsoft.AspNetCore.Authorization;
using Fynanceo.Utils;


namespace Fynanceo.Controllers
{
     [Authorize (Roles = "Administrador,Gerente,Estoquista")]
    
    
    public class EstoqueController : Controller
    {
        private readonly IEstoqueService _estoqueService;
        private readonly IFornecedorService _fornecedorService;
        private readonly IPedidoService _pedidoService;


        public EstoqueController(IEstoqueService estoqueService, IFornecedorService fornecedorService,
            IPedidoService pedidoService)
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
        public async Task<IActionResult> Index(string search, string categoria, StatusEstoque? status)
        {
            // Carregar todos os estoques pela service (lista em memória)
            var estoquesLista = await _estoqueService.ObterTodosEstoquesAsync();

            // Aplicar filtros em memória
            if (!string.IsNullOrEmpty(search))
            {
                var searchNormalized = StringUtils.RemoverAcentosELower(search);
                estoquesLista = estoquesLista
                    .Where(e => (e.Nome != null && StringUtils.RemoverAcentosELower(e.Nome).Contains(searchNormalized)) ||
                                (e.Codigo != null && StringUtils.RemoverAcentosELower(e.Codigo).Contains(searchNormalized)))
                    .ToList();
            }


            if (status.HasValue)
            {
                estoquesLista = estoquesLista
                    .Where(e => e.Status == status.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                estoquesLista = estoquesLista
                    .Where(e => e.Categorias != null && e.Categorias.Contains(categoria, StringComparison.OrdinalIgnoreCase))
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
                    var (estoque, produtoId) = await _estoqueService.CriarEstoqueAsync(model);
                    TempData["Success"] = "Produto cadastrado com sucesso!";

                    // Se um produto relacionado foi criado (TipoItem == "R"), redirecionar para edição do produto
                    if (produtoId.HasValue)
                    {
                        // Só redireciona diretamente para a edição se o usuário atual tiver permissão
                        if (User.IsInRole("Administrador") || User.IsInRole("Gerente"))
                        {
                            return RedirectToAction("Editar", "Produtos", new { id = produtoId.Value });
                        }

                        // Usuário não tem permissão para editar produtos: guardar link para que um administrador possa editar depois
                        var editUrl = Url.Action("Editar", "Produtos", new { id = produtoId.Value });
                        TempData["Info"] = "Produto criado. Um administrador pode completar os detalhes: " + editUrl;
                        return RedirectToAction(nameof(Index));
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao cadastrar produto: {ex.Message}");
                }
            }

            // Recarregar dropdowns em caso de erro
            model.Categorias = await _estoqueService.SomenteCategoriasAsync();

            if (model.FornecedorId.HasValue)
            {
                var fornecedor = await _fornecedorService.obterFornecedorPorId(model.FornecedorId.Value);
                ViewBag.FornecedorSelecionadoNome = fornecedor?.Nome;
            }

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
                Categoria = estoque.Categorias,
                FornecedorId = estoque.FornecedorId,
                Categorias = await _estoqueService.SomenteCategoriasAsync(),
            };

            if (model.FornecedorId.HasValue)
            {
                var fornecedor = await _fornecedorService.obterFornecedorPorId(model.FornecedorId.Value);
                ViewBag.FornecedorSelecionadoNome = fornecedor?.Nome;
            }

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
            model.Categorias = await _estoqueService.SomenteCategoriasAsync();

            if (model.FornecedorId.HasValue)
            {
                var fornecedor = await _fornecedorService.obterFornecedorPorId(model.FornecedorId.Value);
                ViewBag.FornecedorSelecionadoNome = fornecedor?.Nome;
            }

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

                Pedidos = await _pedidoService.ObterPedidosPorStatus("Finalizado"),
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
                    model.Fornecedores =
                        await _fornecedorService.ObterFornecedoresPorStatusAsync(StatusFornecedor.Ativo);
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
        public async Task<IActionResult> CriarInventario(string descricao, string observacao,
            bool incluirTodosProdutos = true)
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
                    await _estoqueService.AdicionarItensInventarioTodosAsync(inventario.Id, apenasAtivos: true,
                        conferido: false);
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

// GET: Estoque/ExportarMovimentacoes
        public async Task<IActionResult> ExportarMovimentacoes(DateTime? dataInicio, DateTime? dataFim, int? produtoId)
        {
            // Mesmos defaults da tela
            dataInicio ??= DateTime.Today.AddDays(-30);
            dataFim ??= DateTime.Today;

            if (dataInicio > dataFim)
                return BadRequest("A data inicial não pode ser maior que a data final.");

            var movimentacoes = await _estoqueService.ObterMovimentacoesAsync(dataInicio, dataFim, produtoId);

            var culture = CultureInfo.GetCultureInfo("pt-BR");
            var sep = ';';
            var sb = new StringBuilder();

            // Cabeçalho (ajuste nomes/ordem se desejar)
            sb.AppendLine(string.Join(sep, new[]
            {
                "Data/Hora", "Produto", "Código", "Tipo", "Quantidade", "Unidade",
                "Custo Unit.", "Valor Total", "Documento", "Fornecedor", "Pedido", "Observações", "Usuário"
            }));

            // Proteção CSV: aspas e quebras de linha
            string Q(string s) => "\"" + (s?.Replace("\"", "\"\"") ?? string.Empty) + "\"";

            foreach (var m in movimentacoes)
            {
                var data = m.DataMovimentacao.ToLocalTime().ToString("dd/MM/yyyy HH:mm", culture);
                var produto = m.Estoque?.Nome ?? string.Empty;
                var codigo = m.Estoque?.Codigo ?? string.Empty;
                var tipo = m.Tipo.ToString(); // se quiser rótulos, mapeie aqui
                var quantidade = m.Quantidade.ToString("N2", culture);
                var unidade = m.Estoque?.UnidadeMedida.ToString() ?? string.Empty; // enum -> texto
                var custoUnit = m.CustoUnitario.ToString("C", culture);
                var valorTotal = m.CustoTotal.ToString("C", culture);
                var documento = m.Documento ?? string.Empty;
                var fornecedor = m.Fornecedor?.Nome ?? string.Empty;
                var pedido = m.PedidoId?.ToString() ?? string.Empty;
                var observacoes = m.Observacao ?? string.Empty; // OBS: singular no modelo
                var usuario = m.Usuario ?? string.Empty;

                var cols = new[]
                {
                    Q(data), Q(produto), Q(codigo), Q(tipo), Q(quantidade), Q(unidade),
                    Q(custoUnit), Q(valorTotal), Q(documento), Q(fornecedor), Q(pedido), Q(observacoes), Q(usuario)
                };
                sb.AppendLine(string.Join(sep, cols));
            }

            // UTF-8 com BOM para abrir corretamente no Excel
            var preamble = Encoding.UTF8.GetPreamble();
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var payload = new byte[preamble.Length + bytes.Length];
            Buffer.BlockCopy(preamble, 0, payload, 0, preamble.Length);
            Buffer.BlockCopy(bytes, 0, payload, preamble.Length, bytes.Length);

            var fileName = $"movimentacoes_{dataInicio:yyyyMMdd}_{dataFim:yyyyMMdd}.csv";
            return File(payload, "text/csv", fileName);
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
        public async Task<IActionResult> ConferirItem(int itemId, int inventarioId, decimal quantidadeFisica,
            string observacao)
        {
            try
            {
                var result =
                    await _estoqueService.ConferirItemAsync(itemId, inventarioId, quantidadeFisica, observacao);
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

        // GET: Estoque/SearchFornecedores?q=abc&limit=20
        [HttpGet]
        public async Task<IActionResult> SearchFornecedores(string q, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
            {
                // Return empty list when query is too short
                return Ok(new List<object>());
            }

            var fornecedores = await _fornecedor_service_Bridge(q, limit);

            // shape results for Select2: { id, text }
            var results = fornecedores.Select(f => new { id = f.Id, text = f.Nome });
            return Ok(results);
        }

        // small bridge to call service method - keeps calling code in one place and handles errors
        private async Task<List<Fynanceo.Models.Fornecedor>> _fornecedor_service_Bridge(string q, int limit)
        {
            try
            {
                return await _fornecedorService.BuscarFornecedoresAsync(q, limit);
            }
            catch
            {
                return new List<Fynanceo.Models.Fornecedor>();
            }
        }
    }
}

