
using Fynanceo.Models;
using Fynanceo.ViewModel.PedidosModel;
using Fynanceo.Models.Enums;


namespace Fynanceo.Service.Interface
{
    public interface IPedidoService
    {
        Task<Pedido> CriarPedido(PedidoViewModel viewModel);
        Task<Pedido> AdicionarItem(int pedidoId, ItemPedidoViewModel item);
        Task<bool> RemoverItem(int itemPedidoId);
        Task<Pedido> AtualizarStatus(int pedidoId, string novoStatus, string usuario);
        Task<Pedido> ObterPedidoCompleto(int pedidoId);
        Task<List<Pedido>> ObterPedidosPorStatus(string status);
        Task<List<Pedido>> ObterPedidosDoDia();
        Task<ItemPedido?> IniciarPreparoItemAsync(int itemPedidoId);
        Task<decimal> CalcularTotalPedido(int pedidoId);

        Task<ItemPedido?> MarcarProntoItemAsync(int itemPedidoId);
        Task<bool> IniciarPreparoTodosAsync(int itemPedidoId);
        Task<bool> MarcarProntoTodosAsync(int itemPedidoId);
        Task<ItemPedido?> EntregueIndividualCozinha(int pedidoId);
        Task<bool> EntregaTodosCozinha(int pedidoId);


        Task<Pedido> ObterPedidoAtivoPorMesa(int mesaId);
        Task<bool> FecharPedidoMesa(int pedidoId, int mesaId);
 
   

        Task<ItemPedido> EnviarItemCozinhaAsync(int itemId);
        Task<ItemPedido> MarcarItemEntregueAsync(int itemId);
        Task<int> EnviarPendentesCozinhaAsync(int pedidoId);

        Task<Pedido> CancelarPedidoAsync(int pedidoId);
        Task<ItemPedido> CancelarItemAsync(int itemId);

        Task<Pedido> FecharPedidoAsync(int pedidoId);

        public  Task<bool> VerificaProdutoJaVendido(int produtoId);
       
        
       // Task AtualizarStatusPedidoAsync(int pedidoId);
       public Task<(bool Success, string Message)> FecharPedidoComPagamentoAsync(
           int pedidoId,
           FormaPagamento formaPagamento,
           decimal? valorRecebido = null,
           string? observacoes = null);


    }
}