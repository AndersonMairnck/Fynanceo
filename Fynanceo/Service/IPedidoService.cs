// Services/IPedidoService.cs
using Fynanceo.Models;
using Fynanceo.ViewModels;

namespace Fynanceo.Services
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
        Task<ItemPedido> IniciarPreparoItemAsync(int itemPedidoId);
        Task<decimal> CalcularTotalPedido(int pedidoId);

        Task<ItemPedido> MarcarProntoItemAsync(int itemPedidoId);
        Task<bool> IniciarPreparoTodosAsync(int itemPedidoId);
        Task<bool> MarcarProntoTodosAsync(int itemPedidoId);
        Task<ItemPedido> EntregueIndividualCozinha(int pedidoId);
        Task<bool> EntregaTodosCozinha(int pedidoId);


    }
}