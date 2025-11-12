// ViewModels/DashboardMesasViewModel.cs
using Fynanceo.Models;
using Fynanceo.Models.Enums;

namespace Fynanceo.ViewModel.PedidosModel
{
    public class DashboardMesasViewModel
    {
        public IEnumerable<Mesa> Mesas { get; set; }
        public IEnumerable<Pedido> PedidosAtivos { get; set; }

        // Estatísticas rápidas
        public int MesasLivres => Mesas.Count(m => m.Status == "Livre");
        public int MesasOcupadas => Mesas.Count(m => m.Status == "Ocupada");
        public int PedidosAtivosCount => PedidosAtivos.Count();
        public int PedidosCozinha => PedidosAtivos.Count(p => p.Status == PedidoStatus.EnviadoCozinha);
    }

    public class GerenciarMesaViewModel
    {
        public Mesa Mesa { get; set; }
        public Pedido PedidoAtivo { get; set; }
        public List<Produto> ProdutosDisponiveis { get; set; }
        public List<string> CategoriasDisponiveis { get; set; }

        // Para adicionar novo item
        public int ProdutoSelecionado { get; set; }
        public int Quantidade { get; set; } = 1;
        public string ObservacoesItem { get; set; }
    }
}