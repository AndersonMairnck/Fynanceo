// ViewModels/DashboardFinanceiroViewModel.cs

// ViewModels/DashboardFinanceiroViewModel.cs

// ViewModels/DashboardFinanceiroViewModel.cs

// ViewModels/DashboardFinanceiroViewModel.cs

// ViewModels/DashboardFinanceiroViewModel.cs

// ViewModels/DashboardFinanceiroViewModel.cs

// ViewModels/DashboardFinanceiroViewModel.cs

// ViewModels/DashboardFinanceiroViewModel.cs
using Fynanceo.Models;


namespace Fynanceo.ViewModel.FinanceirosModel
{
    public class DashboardFinanceiroViewModel
    {
        // Caixa
        public bool CaixaAberto { get; set; }
        public decimal? SaldoCaixa { get; set; }
        public decimal TotalEntradasHoje { get; set; }
        public decimal TotalSaidasHoje { get; set; }

        // Contas
        public int ContasPendentes { get; set; }
        public int ContasAtrasadas { get; set; }
        public decimal TotalContasPagar { get; set; }
        public decimal TotalContasReceber { get; set; }

        // Métricas do Mês
        public decimal FaturamentoMes { get; set; }
        public decimal DespesasMes { get; set; }
        public decimal LucroMes => FaturamentoMes - DespesasMes;
        public decimal TicketMedio { get; set; }

        // Gráficos
        public List<MovimentacaoDiaria> MovimentacoesDiarias { get; set; } = new();
        public List<CategoriaValor> ReceitasPorCategoria { get; set; } = new();
        public List<CategoriaValor> DespesasPorCategoria { get; set; } = new();

        // Alertas
        public List<Conta> ContasProximasVencimento { get; set; } = new();
        public List<Conta> ContasAtrasadasList { get; set; } = new();
    }

    public class MovimentacaoDiaria
    {
        public DateTime Data { get; set; }
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
    }

    public class CategoriaValor
    {
        public string Categoria { get; set; }
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }
}