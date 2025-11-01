// Services/IEntregaService.cs
using Fynanceo.Models;
using Fynanceo.ViewModel.DeliveryModel;

namespace Fynanceo.Service.Interface
{
    public interface IEntregaService
    {
        Task<Entrega> CriarEntrega(int pedidoId);
        Task<Entrega> AtribuirEntregador(int entregaId, int entregadorId);
        Task<Entrega> AtualizarStatusEntrega(int entregaId, string novoStatus, string usuario, string? observacao = null);
        Task<List<Entrega>> ObterEntregasPorStatus(string status);
        Task<List<Entrega>> ObterEntregasDoDia();
        Task<List<Entregador>> ObterEntregadoresDisponiveis();
        Task<decimal> CalcularTaxaEntrega(string endereco, decimal subtotalPedido);
        Task<DashboardDeliveryViewModel> ObterDashboardDelivery();
        Task<bool> AtualizarLocalizacaoEntregador(int entregadorId, decimal latitude, decimal longitude);
    }
}