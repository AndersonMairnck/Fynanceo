// Services/Estoque/IEstoqueService.cs
using Fynanceo.Models;
using Fynanceo.ViewModel.EstoquesModel;

namespace Fynanceo.Service.Interface
{
    public interface IEstoqueService
    {
        // Estoque
        Task<List<Estoque>> ObterTodosEstoquesAsync();
        Task<List<string>> SomenteCategoriasAsync();
        Task<Estoque> ObterEstoquePorIdAsync(int id);
        Task<Estoque> CriarEstoqueAsync(EstoqueViewModel model);
        Task<Estoque> AtualizarEstoqueAsync(int id, EstoqueViewModel model);
        Task<bool> ExcluirEstoqueAsync(int id);
     

        // Movimentações
        Task<List<MovimentacaoEstoque>> ObterMovimentacoesAsync(DateTime? dataInicio, DateTime? dataFim, int? produtoId);
        Task<MovimentacaoEstoque> CriarMovimentacaoAsync(MovimentacaoEstoqueViewModel model);
        Task<decimal> ObterSaldoEstoqueAsync(int produtoId);

        // Alertas
        Task<List<Estoque>> ObterProdutosEstoqueMinimoAsync();
        Task<List<Estoque>> ObterProdutosEstoqueZeroAsync();

        // Dashboard
        Task<DashboardEstoqueViewModel> ObterDadosDashboardAsync();

        // Inventário
        Task<List<Inventario>> ObterInventariosAsync();
        Task<Inventario> ObterInventarioPorIdAsync(int id);
        Task<Inventario> CriarInventarioAsync(InventarioViewModel model);
        Task<Inventario> FecharInventarioAsync(int id);
        Task<bool> AdicionarItemInventarioAsync(int inventarioId, ItemInventarioViewModel model);
        Task AdicionarItensInventarioTodosAsync(int inventarioId, bool apenasAtivos = true, bool conferido = false);
        Task<(bool success, string message)> ConferirItemAsync(int itemId, int inventarioId, decimal quantidadeFisica, string observacao);

        // Integração com Pedidos
        Task<bool> ProcessarSaidaPedidoAsync(int pedidoId);
        Task<bool> ReverterSaidaPedidoAsync(int pedidoId);
    }
}