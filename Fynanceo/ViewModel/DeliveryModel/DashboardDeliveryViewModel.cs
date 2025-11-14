

using Fynanceo.Models;

namespace Fynanceo.ViewModel.DeliveryModel
{
    public class DashboardDeliveryViewModel
    {
        public int TotalEntregasHoje { get; set; }
        public int EntregasPendentes { get; set; }
        public int EntregasEmAndamento { get; set; }
        public int EntregadoresDisponiveis { get; set; }
        public decimal FaturamentoDelivery { get; set; }

        public List<Entrega>? EntregasRecentes { get; set; }
        public List<Entregador>? EntregadoresAtivos { get; set; }
        public List<Entrega>? EntregasUrgentes { get; set; }
    }
}