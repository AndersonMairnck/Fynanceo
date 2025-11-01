// Services/FinanceiroService.cs
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.ContasModel;
using Fynanceo.ViewModel.CaixaModel;
using Fynanceo.ViewModel.FinanceirosModel;

namespace Fynanceo.Services
{
    public class FinanceiroService : IFinanceiroService
    {
        private readonly AppDbContext _context;

        public FinanceiroService(AppDbContext context)
        {
            _context = context;
        }

        // CAIXA
        public async Task<Caixa> AbrirCaixa(CaixaViewModel viewModel)
        {
            // Verificar se já existe caixa aberto
            var caixaAberto = await ObterCaixaAberto();
            if (caixaAberto != null)
                throw new InvalidOperationException("Já existe um caixa aberto");

            var caixa = new Caixa
            {
                DataAbertura = DateTime.Now,
                SaldoInicial = viewModel.SaldoInicial,
                TotalEntradas = 0,
                TotalSaidas = 0,
                UsuarioAberturaId = 1, // Temporário
                UsuarioAberturaNome = "Sistema",
                Observacoes = viewModel.Observacoes,
                Fechado = false
            };

            _context.Caixas.Add(caixa);
            await _context.SaveChangesAsync();

            return caixa;
        }

        public async Task<Caixa> FecharCaixa(FechamentoCaixaViewModel viewModel)
        {
            var caixa = await _context.Caixas
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => c.Id == viewModel.CaixaId && !c.Fechado);

            if (caixa == null)
                throw new ArgumentException("Caixa não encontrado ou já fechado");

            caixa.DataFechamento = DateTime.Now;
            caixa.SaldoFisico = viewModel.SaldoFisico;
            caixa.UsuarioFechamentoId = 1; // Temporário
            caixa.UsuarioFechamentoNome = "Sistema";
            caixa.Fechado = true;

            // Atualizar totais baseado nas movimentações
            caixa.TotalEntradas = caixa.Movimentacoes
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .Sum(m => m.Valor);

            caixa.TotalSaidas = caixa.Movimentacoes
                .Where(m => m.Tipo == TipoMovimentacao.Saida)
                .Sum(m => m.Valor);

            await _context.SaveChangesAsync();
            return caixa;
        }

        public async Task<Caixa?> ObterCaixaAberto()
        {
            return await _context.Caixas
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => !c.Fechado);
        }

        public async Task<List<Caixa>> ObterCaixasPeriodo(DateTime inicio, DateTime fim)
        {
            return await _context.Caixas
                .Include(c => c.Movimentacoes)
                .Where(c => c.DataAbertura.Date >= inicio.Date && c.DataAbertura.Date <= fim.Date)
                .OrderByDescending(c => c.DataAbertura)
                .ToListAsync();
        }

        // MOVIMENTAÇÕES
        public async Task<MovimentacaoCaixa> AdicionarMovimentacao(MovimentacaoViewModel viewModel)
        {
            var caixa = await ObterCaixaAberto();
            if (caixa == null)
                throw new InvalidOperationException("Nenhum caixa aberto encontrado");

            var movimentacao = new MovimentacaoCaixa
            {
                CaixaId = caixa.Id,
                Tipo = viewModel.Tipo,
                Valor = viewModel.Valor,
                FormaPagamento = viewModel.FormaPagamento,
                Categoria = viewModel.Categoria,
                Descricao = viewModel.Descricao,
                Observacoes = viewModel.Observacoes,
                IsSangria = viewModel.IsSangria,
                IsSuprimento = viewModel.IsSuprimento,
                DataMovimentacao = DateTime.Now,
                UsuarioId = 1, // Temporário
                UsuarioNome = "Sistema"
            };

            _context.MovimentacoesCaixa.Add(movimentacao);
            await _context.SaveChangesAsync();

            return movimentacao;
        }

        public async Task<List<MovimentacaoCaixa>> ObterMovimentacoesCaixa(int caixaId)
        {
            return await _context.MovimentacoesCaixa
                .Where(m => m.CaixaId == caixaId)
                .OrderByDescending(m => m.DataMovimentacao)
                .ToListAsync();
        }

        public async Task<decimal> ObterSaldoCaixa(int caixaId)
        {
            var caixa = await _context.Caixas
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => c.Id == caixaId);

            if (caixa == null) return 0;

            return caixa.SaldoFinal;
        }

        // CONTAS
        public async Task<Conta> CriarConta(ContaViewModel viewModel)
        {
            var conta = new Conta
            {
                Descricao = viewModel.Descricao,
                Tipo = viewModel.Tipo,
                Categoria = viewModel.Categoria,
                Valor = viewModel.Valor,
                DataVencimento = viewModel.DataVencimento,
                FormaPagamento = viewModel.FormaPagamento,
                FornecedorId = viewModel.FornecedorId,
                ParcelaAtual = viewModel.ParcelaAtual,
                TotalParcelas = viewModel.TotalParcelas,
                Observacoes = viewModel.Observacoes,
                Status = StatusConta.Pendente,
                DataCriacao = DateTime.Now
            };

            _context.Contas.Add(conta);
            await _context.SaveChangesAsync();

            return conta;
        }

        public async Task<Conta> PagarConta(PagamentoContaViewModel viewModel)
        {
            var conta = await _context.Contas.FindAsync(viewModel.ContaId);
            if (conta == null)
                throw new ArgumentException("Conta não encontrada");

            var statusAnterior = conta.Status;

            conta.ValorPago = viewModel.ValorPago;
            conta.DataPagamento = viewModel.DataPagamento;
            conta.FormaPagamento = viewModel.FormaPagamento;
            conta.Status = StatusConta.Paga;
            conta.DataAlteracao = DateTime.Now;

            // Registrar movimentação no caixa se estiver aberto
            var caixaAberto = await ObterCaixaAberto();
            if (caixaAberto != null && conta.Tipo == TipoMovimentacao.Saida)
            {
                var movimentacao = new MovimentacaoCaixa
                {
                    CaixaId = caixaAberto.Id,
                    Tipo = TipoMovimentacao.Saida,
                    Valor = viewModel.ValorPago,
                    FormaPagamento = viewModel.FormaPagamento,
                    Categoria = conta.Categoria,
                    Descricao = $"Pagamento: {conta.Descricao}",
                    Observacoes = viewModel.Observacoes,
                    Id = conta.Id,
                    DataMovimentacao = DateTime.Now,
                    UsuarioId = 1,
                    UsuarioNome = "Sistema"
                };

                _context.MovimentacoesCaixa.Add(movimentacao);
            }

            // Registrar histórico
            var historico = new MovimentacaoConta
            {
                ContaId = conta.Id,
                StatusAnterior = statusAnterior,
                StatusNovo = StatusConta.Paga,
                ValorPago = viewModel.ValorPago,
                DataPagamento = viewModel.DataPagamento,
                Observacao = viewModel.Observacoes,
                UsuarioId = 1,
                UsuarioNome = "Sistema",
                DataAlteracao = DateTime.Now
            };

            _context.MovimentacaoContas.Add(historico);
            await _context.SaveChangesAsync();

            return conta;
        }

        public async Task<List<Conta>> ObterContasPendentes()
        {
            return await _context.Contas
                .Include(c => c.Fornecedor)
                .Where(c => c.Status == StatusConta.Pendente)
                .OrderBy(c => c.DataVencimento)
                .ToListAsync();
        }

        public async Task<List<Conta>> ObterContasAtrasadas()
        {
            return await _context.Contas
                .Include(c => c.Fornecedor)
                .Where(c => c.Status == StatusConta.Pendente && c.DataVencimento < DateTime.Today)
                .OrderBy(c => c.DataVencimento)
                .ToListAsync();
        }

        public async Task<List<Conta>> ObterContasPorPeriodo(DateTime inicio, DateTime fim)
        {
            return await _context.Contas
                .Include(c => c.Fornecedor)
                .Where(c => c.DataVencimento >= inicio && c.DataVencimento <= fim)
                .OrderBy(c => c.DataVencimento)
                .ToListAsync();
        }

        // DASHBOARD
        public async Task<DashboardFinanceiroViewModel> ObterDashboardFinanceiro()
        {
            var dashboard = new DashboardFinanceiroViewModel();
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            // Caixa
            var caixaAberto = await ObterCaixaAberto();
            dashboard.CaixaAberto = caixaAberto != null;
            dashboard.SaldoCaixa = caixaAberto?.SaldoFinal;

            if (caixaAberto != null)
            {
                dashboard.TotalEntradasHoje = caixaAberto.Movimentacoes
                    .Where(m => m.Tipo == TipoMovimentacao.Entrada && m.DataMovimentacao.Date == hoje)
                    .Sum(m => m.Valor);

                dashboard.TotalSaidasHoje = caixaAberto.Movimentacoes
                    .Where(m => m.Tipo == TipoMovimentacao.Saida && m.DataMovimentacao.Date == hoje)
                    .Sum(m => m.Valor);
            }

            // Contas
            dashboard.ContasPendentes = await _context.Contas
                .CountAsync(c => c.Status == StatusConta.Pendente);

            dashboard.ContasAtrasadas = await _context.Contas
                .CountAsync(c => c.Status == StatusConta.Pendente && c.DataVencimento < hoje);

            dashboard.TotalContasPagar = await _context.Contas
                .Where(c => c.Tipo == TipoMovimentacao.Saida && c.Status == StatusConta.Pendente)
                .SumAsync(c => c.Valor);

            dashboard.TotalContasReceber = await _context.Contas
                .Where(c => c.Tipo == TipoMovimentacao.Entrada && c.Status == StatusConta.Pendente)
                .SumAsync(c => c.Valor);

            // Métricas do Mês
            var movimentacoesMes = await _context.MovimentacoesCaixa
                .Where(m => m.DataMovimentacao >= inicioMes && m.DataMovimentacao <= fimMes)
                .ToListAsync();

            dashboard.FaturamentoMes = movimentacoesMes
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .Sum(m => m.Valor);

            dashboard.DespesasMes = movimentacoesMes
                .Where(m => m.Tipo == TipoMovimentacao.Saida)
                .Sum(m => m.Valor);

            // Ticket Médio
            var pedidosMes = await _context.Pedidos
                .Where(p => p.DataAbertura >= inicioMes && p.DataAbertura <= fimMes && p.Status == Fynanceo.Models.Enums.PedidoStatus.Fechado)
                .ToListAsync();

            dashboard.TicketMedio = pedidosMes.Any() ?
                pedidosMes.Average(p => p.Total) : 0;

            // Alertas
            dashboard.ContasProximasVencimento = await _context.Contas
                .Include(c => c.Fornecedor)
                .Where(c => c.Status == StatusConta.Pendente &&
                           c.DataVencimento >= hoje &&
                           c.DataVencimento <= hoje.AddDays(7))
                .OrderBy(c => c.DataVencimento)
                .Take(10)
                .ToListAsync();

            dashboard.ContasAtrasadasList = await _context.Contas
                .Include(c => c.Fornecedor)
                .Where(c => c.Status == StatusConta.Pendente && c.DataVencimento < hoje)
                .OrderBy(c => c.DataVencimento)
                .Take(10)
                .ToListAsync();

            // Gráficos
            dashboard.MovimentacoesDiarias = await ObterMovimentacoesUltimos30Dias();

            return dashboard;
        }

        public async Task<List<MovimentacaoDiaria>> ObterMovimentacoesUltimos30Dias()
        {
            var dataInicio = DateTime.Today.AddDays(-30);
            var movimentacoes = await _context.MovimentacoesCaixa
                .Where(m => m.DataMovimentacao >= dataInicio)
                .ToListAsync();

            return movimentacoes
                .GroupBy(m => m.DataMovimentacao.Date)
                .Select(g => new MovimentacaoDiaria
                {
                    Data = g.Key,
                    Entradas = g.Where(m => m.Tipo == TipoMovimentacao.Entrada).Sum(m => m.Valor),
                    Saidas = g.Where(m => m.Tipo == TipoMovimentacao.Saida).Sum(m => m.Valor)
                })
                .OrderBy(m => m.Data)
                .ToList();
        }
    }
}