// Services/Estoque/EstoqueService.cs
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.ViewModel.EstoquesModel;
using Fynanceo.Service.Interface;
using Fynanceo.Models.Enums;



namespace Fynanceo.Service
{
    public class EstoqueService : IEstoqueService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EstoqueService> _logger;

        public EstoqueService(AppDbContext context, ILogger<EstoqueService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ESTOQUE - CRUD
        public async Task<List<Estoque>> ObterTodosEstoquesAsync()
        {
            return await _context.Estoques
              
                .Include(e => e.Fornecedor)
                .Where(e => e.Status == StatusEstoque.Ativo)
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        public async Task<Estoque> ObterEstoquePorIdAsync(int id)
        {
            try
            {
                return await _context.Estoques
                        
               
                .Include(e => e.Fornecedor)
                .Include(e => e.Movimentacoes)
                .Include(e => e.ProdutoIngredientes)
                .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
         
        }

        public async Task<Estoque> CriarEstoqueAsync(EstoqueViewModel model)
        {
            try
            {
                var estoque = new Estoque
                {
                    Nome = model.Nome,
                    Codigo = model.Codigo,
                    Descricao = model.Descricao,
                    EstoqueAtual = model.EstoqueAtual,
                    EstoqueMinimo = model.EstoqueMinimo,
                    EstoqueMaximo = model.EstoqueMaximo,
                    CustoUnitario = model.CustoUnitario,
                    UnidadeMedida = model.StatusUnidadeMedida,
               

                    Status = model.Status,
                    Categorias = model.Categoria,
             
                    FornecedorId = model.FornecedorId,
                    DataCriacao = DateTime.UtcNow
                };

                _context.Estoques.Add(estoque);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Estoque criado: {estoque.Nome} (ID: {estoque.Id})");
                return estoque;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
          
        }

        public async Task<Estoque> AtualizarEstoqueAsync(int id, EstoqueViewModel model)
        {
            var estoque = await _context.Estoques.FindAsync(id);
            if (estoque == null)
                throw new ArgumentException("Produto não encontrado");

            estoque.Nome = model.Nome;
            estoque.Codigo = model.Codigo;
            estoque.Descricao = model.Descricao;
            estoque.EstoqueMinimo = model.EstoqueMinimo;
            estoque.EstoqueMaximo = model.EstoqueMaximo;
            estoque.CustoUnitario = model.CustoUnitario;
            estoque.UnidadeMedida = model.StatusUnidadeMedida;
            estoque.Status = model.Status;
         
            estoque.FornecedorId = model.FornecedorId;
            estoque.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Estoque atualizado: {estoque.Nome} (ID: {estoque.Id})");
            return estoque;
        }

        public async Task<bool> ExcluirEstoqueAsync(int id)
        {
            var estoque = await _context.Estoques.FindAsync(id);
            if (estoque == null)
                return false;

            // Verificar se há movimentações ou ingredientes vinculados
            var temMovimentacoes = await _context.MovimentacoesEstoque.AnyAsync(m => m.EstoqueId == id);
            var temIngredientes = await _context.ProdutoIngredientes.AnyAsync(pi => pi.EstoqueId == id);

            if (temMovimentacoes || temIngredientes)
            {
                // Soft delete - apenas inativa
                estoque.Status = StatusEstoque.Inativo;
                estoque.DataAtualizacao = DateTime.UtcNow;
            }
            else
            {
                // Hard delete - remove completamente
                _context.Estoques.Remove(estoque);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Estoque excluído/inativado: {estoque.Nome} (ID: {estoque.Id})");
            return true;
        }

        // MOVIMENTAÇÕES
        public async Task<List<MovimentacaoEstoque>> ObterMovimentacoesAsync(DateTime? dataInicio, DateTime? dataFim, int? produtoId)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Estoque)
                .Include(m => m.Fornecedor)
                .Include(m => m.Pedido)
                .AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(m => m.DataMovimentacao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(m => m.DataMovimentacao <= dataFim.Value.AddDays(1).AddSeconds(-1));

            if (produtoId.HasValue)
                query = query.Where(m => m.EstoqueId == produtoId.Value);

            return await query
                .OrderByDescending(m => m.DataMovimentacao)
                .ToListAsync();
        }

        public async Task<MovimentacaoEstoque> CriarMovimentacaoAsync(MovimentacaoEstoqueViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var estoque = await _context.Estoques.FindAsync(model.EstoqueId);
                if (estoque == null)
                    throw new ArgumentException("Produto não encontrado");

                var movimentacao = new MovimentacaoEstoque
                {
                    EstoqueId = model.EstoqueId,
                    Tipo = model.Tipo,
                    Quantidade = model.Quantidade,
                    CustoUnitario = model.CustoUnitario,
                    CustoTotal = model.Quantidade * model.CustoUnitario,
                    Documento = model.Documento,
                    Observacao = model.Observacao,
                    FornecedorId = model.FornecedorId,
                    PedidoId = model.PedidoId,
                    DataMovimentacao = DateTime.UtcNow,
                    Usuario = "Sistema" // TODO: Integrar com autenticação
                };

                // Atualizar estoque atual
                if (model.Tipo == TipoMovimentacaoEstoque.Entrada ||
                    model.Tipo == TipoMovimentacaoEstoque.Ajuste && model.Quantidade > 0)
                {
                    estoque.EstoqueAtual += model.Quantidade;
                }
                else if (model.Tipo == TipoMovimentacaoEstoque.Saida ||
                         model.Tipo == TipoMovimentacaoEstoque.Perda ||
                         (model.Tipo == TipoMovimentacaoEstoque.Ajuste && model.Quantidade < 0))
                {
                    estoque.EstoqueAtual -= model.Quantidade;

                    // Validar estoque negativo
                    if (estoque.EstoqueAtual < 0)
                    {
                        throw new InvalidOperationException($"Estoque insuficiente para {estoque.Nome}. Estoque atual: {estoque.EstoqueAtual + model.Quantidade}, Requerido: {model.Quantidade}");
                    }
                }

                // Atualizar custo unitário se for entrada
                if (model.Tipo == TipoMovimentacaoEstoque.Entrada)
                {
                    estoque.CustoUnitario = model.CustoUnitario;
                }

                estoque.DataAtualizacao = DateTime.UtcNow;

                _context.MovimentacoesEstoque.Add(movimentacao);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Movimentação criada: {model.Tipo} - {model.Quantidade} unidades de {estoque.Nome}");
                return movimentacao;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao criar movimentação de estoque");
                throw;
            }
        }

        public async Task<decimal> ObterSaldoEstoqueAsync(int produtoId)
        {
            var estoque = await _context.Estoques.FindAsync(produtoId);
            return estoque?.EstoqueAtual ?? 0;
        }

        // ALERTAS
        public async Task<List<Estoque>> ObterProdutosEstoqueMinimoAsync()
        {
            return await _context.Estoques
                .Where(e => e.Status == StatusEstoque.Ativo &&
                           e.EstoqueAtual <= e.EstoqueMinimo)
                .OrderBy(e => e.EstoqueAtual)
                .ToListAsync();
        }

        public async Task<List<Estoque>> ObterProdutosEstoqueZeroAsync()
        {
            return await _context.Estoques
                .Where(e => e.Status == StatusEstoque.Ativo &&
                           e.EstoqueAtual == 0)
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        // DASHBOARD
        public async Task<DashboardEstoqueViewModel> ObterDadosDashboardAsync()
        {
            var dashboard = new DashboardEstoqueViewModel();

            var produtos = await _context.Estoques
                .Where(e => e.Status == StatusEstoque.Ativo)
                .ToListAsync();

            dashboard.TotalProdutos = produtos.Count;
            dashboard.ProdutosEstoqueMinimo = produtos.Count(p => p.EstoqueAtual <= p.EstoqueMinimo && p.EstoqueAtual > 0);
            dashboard.ProdutosEstoqueZero = produtos.Count(p => p.EstoqueAtual == 0);
            dashboard.ValorTotalEstoque = produtos.Sum(p => p.EstoqueAtual * p.CustoUnitario);

            var hoje = DateTime.Today;
            dashboard.MovimentacoesHoje = await _context.MovimentacoesEstoque
                .CountAsync(m => m.DataMovimentacao.Date == hoje);

            dashboard.ProdutosBaixoEstoque = await ObterProdutosEstoqueMinimoAsync();

            dashboard.UltimasMovimentacoes = await _context.MovimentacoesEstoque
                .Include(m => m.Estoque)
                .OrderByDescending(m => m.DataMovimentacao)
                .Take(10)
                .ToListAsync();

            // Dados para gráficos
            dashboard.ValorPorCategoria = await _context.Estoques
              
                .Where(e => e.Status == StatusEstoque.Ativo)
                .GroupBy(e => e.Categorias != null ? e.Categorias : "Sem Categoria")
                .Select(g => new { Categoria = g.Key, Valor = g.Sum(e => e.EstoqueAtual * e.CustoUnitario) })
                .ToDictionaryAsync(x => x.Categoria, x => x.Valor);

            dashboard.MovimentacoesPorTipo = await _context.MovimentacoesEstoque
                .Where(m => m.DataMovimentacao.Date == hoje)
                .GroupBy(m => m.Tipo)
                .Select(g => new { Tipo = g.Key.ToString(), Quantidade = g.Count() })
                .ToDictionaryAsync(x => x.Tipo, x => x.Quantidade);

            return dashboard;
        }

        // INVENTÁRIO
        public async Task<List<Inventario>> ObterInventariosAsync()
        {
            return await _context.Inventarios
                .Include(i => i.Itens)
                .ThenInclude(item => item.Estoque)
                .OrderByDescending(i => i.DataAbertura)
                .ToListAsync();
        }

        public async Task<Inventario> ObterInventarioPorIdAsync(int id)
        {
            return await _context.Inventarios
                .Include(i => i.Itens)
                .ThenInclude(item => item.Estoque)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Inventario> CriarInventarioAsync(InventarioViewModel model)
        {
            var inventario = new Inventario
            {
                Descricao = model.Descricao,
                Observacao = model.Observacao,
                Status = StatusInventario.Aberto,
                DataAbertura = DateTime.UtcNow
            };

            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Inventário criado: {inventario.Descricao} (ID: {inventario.Id})");
            return inventario;
        }

        public async Task<Inventario> FecharInventarioAsync(int id)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Itens)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
                throw new ArgumentException("Inventário não encontrado");

            // Processar diferenças do inventário
            foreach (var item in inventario.Itens.Where(i => i.Conferido))
            {
                if (item.Diferenca != 0)
                {
                    // Criar movimentação de ajuste
                    var ajuste = new MovimentacaoEstoqueViewModel
                    {
                        EstoqueId = item.EstoqueId,
                        Tipo = TipoMovimentacaoEstoque.Ajuste,
                        Quantidade = item.Diferenca,
                        CustoUnitario = item.CustoUnitario,
                        Observacao = $"Ajuste de inventário #{inventario.Id}",
                        UsuarioNome = "Sistema"
                    };

                    await CriarMovimentacaoAsync(ajuste);
                }
            }

            inventario.Status = StatusInventario.Concluido;
            inventario.DataFechamento = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Inventário fechado: {inventario.Descricao} (ID: {inventario.Id})");
            return inventario;
        }

        public async Task<bool> AdicionarItemInventarioAsync(int inventarioId, ItemInventarioViewModel model)
        {
            var inventario = await _context.Inventarios.FindAsync(inventarioId);
            if (inventario == null || inventario.Status != StatusInventario.Aberto)
                return false;

            var estoque = await _context.Estoques.FindAsync(model.EstoqueId);
            if (estoque == null)
                return false;

            var item = new ItemInventario
            {
                InventarioId = inventarioId,
                EstoqueId = model.EstoqueId,
                QuantidadeSistema = estoque.EstoqueAtual,
                QuantidadeFisica = model.QuantidadeFisica,
                CustoUnitario = estoque.CustoUnitario,
                Observacao = model.Observacao,
                Conferido = true
            };

            _context.ItensInventario.Add(item);
            await _context.SaveChangesAsync();

            return true;
        }

        // INTEGRAÇÃO COM PEDIDOS
        public async Task<bool> ProcessarSaidaPedidoAsync(int pedidoId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .ThenInclude(p => p.ProdutoIngredientes)
              //  .ThenInclude(i => i.Estoque)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in pedido.Itens)
                {
                    if (item.Produto?.ProdutoIngredientes != null)
                    {
                        foreach (var ingrediente in item.Produto.ProdutoIngredientes)
                        {
                            var movimentacao = new MovimentacaoEstoqueViewModel
                            {
                             //   EstoqueId = ingrediente.Estoque.Id,
                                Tipo = TipoMovimentacaoEstoque.Saida,
                                Quantidade = ingrediente.Quantidade * item.Quantidade,
                             //   CustoUnitario = ingrediente.Estoque.CustoUnitario,
                                PedidoId = pedidoId,
                                Observacao = $"Saída automática - Pedido #{pedidoId} - {item.Produto.Nome}"
                            };

                            await CriarMovimentacaoAsync(movimentacao);
                        }
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao processar saída de estoque para pedido {pedidoId}");
                return false;
            }
        }

        public async Task<bool> ReverterSaidaPedidoAsync(int pedidoId)
        {
            var movimentacoes = await _context.MovimentacoesEstoque
                .Where(m => m.PedidoId == pedidoId && m.Tipo == TipoMovimentacaoEstoque.Saida)
                .ToListAsync();

            if (!movimentacoes.Any()) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var movimentacao in movimentacoes)
                {
                    // Criar entrada para reverter a saída
                    var reversao = new MovimentacaoEstoqueViewModel
                    {
                        EstoqueId = movimentacao.EstoqueId,
                        Tipo = TipoMovimentacaoEstoque.Entrada,
                        Quantidade = movimentacao.Quantidade,
                        CustoUnitario = movimentacao.CustoUnitario,
                        PedidoId = pedidoId,
                        Observacao = $"Reversão de saída - Pedido #{pedidoId}"
                    };

                    await CriarMovimentacaoAsync(reversao);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Erro ao reverter saída de estoque para pedido {pedidoId}");
                return false;
            }
        }
    }
}