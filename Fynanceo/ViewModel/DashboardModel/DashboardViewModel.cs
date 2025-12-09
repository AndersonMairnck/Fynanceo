using System;

namespace Fynanceo.ViewModel.DashboardModel
{
    public class DashboardViewModel
    {
        public decimal VendasHoje { get; set; }
        public int PedidosAtivos { get; set; }
        public int MesasOcupadas { get; set; }
        public decimal TicketMedio { get; set; }
        public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
    }
}
