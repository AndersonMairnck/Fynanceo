// Services/IFinanceiroService.cs
using Fynanceo.Models;
using Fynanceo.ViewModel.CaixaModel;
using Fynanceo.ViewModel.ContasModel;
using Fynanceo.ViewModel.FinanceirosModel;

namespace Fynanceo.Service.Interface
{
    public interface IFinanceiroService
    {
        // Caixa
        Task<Caixa> AbrirCaixa(CaixaViewModel viewModel);
        Task<Caixa> FecharCaixa(FechamentoCaixaViewModel viewModel);
        Task<Caixa?> ObterCaixaAberto();
        Task<List<Caixa>> ObterCaixasPeriodo(DateTime inicio, DateTime fim);

        // Movimentações
        Task<MovimentacaoCaixa> AdicionarMovimentacao(MovimentacaoViewModel viewModel);
        Task<List<MovimentacaoCaixa>> ObterMovimentacoesCaixa(int caixaId);
        Task<decimal> ObterSaldoCaixa(int caixaId);

        // Contas
        Task<Conta> CriarConta(ContaViewModel viewModel);
        Task<Conta> PagarConta(PagamentoContaViewModel viewModel);
        Task<List<Conta>> ObterContasPendentes();
        Task<List<Conta>> ObterContasAtrasadas();
        Task<List<Conta>> ObterContasPorPeriodo(DateTime inicio, DateTime fim);

        // Dashboard
        Task<DashboardFinanceiroViewModel> ObterDashboardFinanceiro();
        Task<List<MovimentacaoDiaria>> ObterMovimentacoesUltimos30Dias();
    }
}