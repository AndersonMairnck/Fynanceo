// Services/EntregaService.cs
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.DeliveryModel;

namespace Fynanceo.Services
{
    public class EntregaService : IEntregaService
    {
        private readonly AppDbContext _context;

        public EntregaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Entrega> CriarEntrega(int pedidoId)
        {
            try
            {

           
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EnderecoEntrega)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new ArgumentException("Pedido não encontrado");

            if (pedido.EnderecoEntrega == null)
                throw new ArgumentException("Pedido não tem endereço de entrega");

            // Gerar código de verificação
            var codigoVerificacao = new Random().Next(1000, 9999).ToString();

            var entrega = new Entrega
            {
                PedidoId = pedidoId,
                EnderecoEntregaId = pedido.EnderecoEntrega.Id,
                Status = StatusEntrega.AguardandoEntregador,
                EnderecoCompleto = pedido.EnderecoEntrega.ToString(),
                Complemento = pedido.EnderecoEntrega.Complemento,
                Referencia = pedido.EnderecoEntrega.Referencia,
                TaxaEntrega = pedido.TaxaEntrega,
                ComissaoEntregador = await CalcularComissao(pedido.TaxaEntrega),
                CodigoVerificacao = codigoVerificacao,
                DataCriacao = DateTime.UtcNow
            };

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            // Adicionar ao histórico
            await AdicionarHistoricoEntrega(entrega.Id, "Nova", entrega.Status.ToString(), "Sistema");

            return entrega;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Entrega> AtribuirEntregador(int entregaId, int entregadorId)
        {
            var entrega = await _context.Entregas.FindAsync(entregaId);
            var entregador = await _context.Entregadores.FindAsync(entregadorId);

            if (entrega == null || entregador == null)
                throw new ArgumentException("Entrega ou entregador não encontrado");

            if (entregador.Status != StatusEntregador.Disponivel)
                throw new ArgumentException("Entregador não está disponível");
            // status da entrega
            var statusAnterior = entrega.Status.ToString();

            entrega.EntregadorId = entregadorId;
            entrega.Status = StatusEntrega.RetiradoParaEntrega;
            entrega.DataSaiuEntrega = DateTime.UtcNow;
            entrega.DataPrevisao = DateTime.UtcNow.AddMinutes(30); // Baseado na distância

           // entregador.Status = StatusEntregador.Entregando;

            await _context.SaveChangesAsync();

            await AdicionarHistoricoEntrega(entregaId, statusAnterior, entrega.Status.ToString(), "Sistema",
                $"Entregador: {entregador.Nome}");

            return await ObterEntregaCompleta(entregaId);
        }

        public async Task<Entrega> AtualizarStatusEntrega(int entregaId, string novoStatus, string usuario, string? observacao = null)
        {
            var entrega = await _context.Entregas.FindAsync(entregaId);
            if (entrega == null)
                throw new ArgumentException("Entrega não encontrada");

            var statusAnterior = entrega.Status.ToString();

            if (Enum.TryParse<StatusEntrega>(novoStatus, out var status))
            {
                entrega.Status = status;

                // Atualizar timestamps
                switch (status)
                {
                    case StatusEntrega.EmRota:
                       
                       // ocupar entregador
                        if (entrega.EntregadorId.HasValue)
                        {
                            var entregador = await _context.Entregadores.FindAsync(entrega.EntregadorId.Value);
                            if (entregador != null)
                            {
                                entregador.Status = StatusEntregador.Entregando;
                            
                            }
                        }
                        break;
                    case StatusEntrega.Entregue:
                        entrega.DataEntrega = DateTime.UtcNow;
                        // Liberar entregador
                        if (entrega.EntregadorId.HasValue)
                        {
                            var entregador = await _context.Entregadores.FindAsync(entrega.EntregadorId.Value);
                            if (entregador != null)
                            {
                                entregador.Status = StatusEntregador.Disponivel;
                                entregador.TotalEntregas++;
                                entregador.UltimaAtualizacao = DateTime.UtcNow;
                            }
                        }
                        break;
                    case StatusEntrega.Cancelada:
                        entrega.DataCancelamento = DateTime.UtcNow;
                        // Liberar entregador se houver
                        if (entrega.EntregadorId.HasValue)
                        {
                            var entregador = await _context.Entregadores.FindAsync(entrega.EntregadorId.Value);
                            if (entregador != null)
                            {
                                entregador.Status = StatusEntregador.Disponivel;
                                entregador.UltimaAtualizacao = DateTime.UtcNow;
                            }
                        }
                        break;
                }

                await _context.SaveChangesAsync();
                await AdicionarHistoricoEntrega(entregaId, statusAnterior, novoStatus, usuario, observacao);
            }

            return await ObterEntregaCompleta(entregaId);
        }

        public async Task<List<Entrega>> ObterEntregasPorStatus(string status)
        {
            if (Enum.TryParse<StatusEntrega>(status, out var statusEnum))
            {
                return await _context.Entregas
                    .Include(e => e.Pedido)
                     .Include(e => e.EnderecoEntrega)
                        .ThenInclude(p => p.Cliente)
                      
                    .Include(e => e.Entregador)
                    .Where(e => e.Status == statusEnum)
                    .OrderBy(e => e.DataCriacao)
                    .ToListAsync();
            }

            return new List<Entrega>();
        }

        public async Task<List<Entrega>> ObterEntregasDoDia()
        {
        

            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            return await _context.Entregas
                .Include(e => e.Pedido)
                .Include(e => e.EnderecoEntrega)
                    .ThenInclude(p => p.Cliente)
                .Include(e => e.Entregador)
                .Where(e => e.DataCriacao >= hoje && e.DataCriacao < amanha)
                .OrderByDescending(e => e.DataCriacao)
                .ToListAsync();
        }

        public async Task<List<Entregador>> ObterEntregadoresDisponiveis()
        {
            return await _context.Entregadores
                .Where(e => e.Status == StatusEntregador.Disponivel && e.Ativo)
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        public async Task<decimal> CalcularTaxaEntrega(string endereco, decimal subtotalPedido)
        {
            // Obter configurações
            var config = await _context.ConfiguracoesDelivery.FirstOrDefaultAsync();
            if (config == null)
                return 5.00m; // Taxa padrão

            // Verificar se tem direito a entrega grátis
            if (subtotalPedido >= config.ValorMinimoGratis)
                return 0;

            // Aqui integraria com API de mapas para calcular distância
            // Por enquanto retorna taxa base
            return config.TaxaBase;
        }

        public async Task<DashboardDeliveryViewModel> ObterDashboardDelivery()
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            var totalEntregasHoje = await _context.Entregas
                .CountAsync(e => e.DataCriacao >= hoje && e.DataCriacao < amanha);

            

            var entregasPendentes = await _context.Entregas
                .CountAsync(e => e.Status == StatusEntrega.AguardandoEntregador);

            var entregasEmAndamento = await _context.Entregas
                .CountAsync(e => e.Status == StatusEntrega.RetiradoParaEntrega || e.Status == StatusEntrega.EmRota);

            var entregadoresDisponiveis = await _context.Entregadores
                .CountAsync(e => e.Status == StatusEntregador.Disponivel && e.Ativo);

            var faturamentoDelivery = await _context.Entregas
                .Where(e => e.DataCriacao == hoje && e.Status == StatusEntrega.Entregue)
                .SumAsync(e => e.TaxaEntrega);

            var entregasRecentes = await _context.Entregas
                .Include(e => e.Pedido)
                .Include(e => e.Entregador)
                .Where(e => e.DataCriacao >= hoje && e.DataCriacao < amanha)
                .OrderByDescending(e => e.DataCriacao)
                .Take(10)
                .ToListAsync();

            var entregadoresAtivos = await _context.Entregadores
                .Where(e => e.Ativo)
                .OrderBy(e => e.Nome)
                .ToListAsync();

            var entregasUrgentes = await _context.Entregas
                .Include(e => e.Pedido)
                .Include(e => e.EnderecoEntrega)
                .Where(e => e.Status == StatusEntrega.AguardandoEntregador &&
                           e.DataCriacao < DateTime.UtcNow.AddMinutes(-10))
                .OrderBy(e => e.DataCriacao)
                .ToListAsync();

            return new DashboardDeliveryViewModel
            {
                TotalEntregasHoje = totalEntregasHoje,
                EntregasPendentes = entregasPendentes,
                EntregasEmAndamento = entregasEmAndamento,
                EntregadoresDisponiveis = entregadoresDisponiveis,
                FaturamentoDelivery = faturamentoDelivery,
                EntregasRecentes = entregasRecentes,
                EntregadoresAtivos = entregadoresAtivos,
                EntregasUrgentes = entregasUrgentes
            };
        }

        public async Task<bool> AtualizarLocalizacaoEntregador(int entregadorId, decimal latitude, decimal longitude)
        {
            var entregador = await _context.Entregadores.FindAsync(entregadorId);
            if (entregador == null) return false;

            entregador.Latitude = latitude;
            entregador.Longitude = longitude;
            entregador.UltimaAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<decimal> CalcularComissao(decimal taxaEntrega)
        {
            var config = await _context.ConfiguracoesDelivery.FirstOrDefaultAsync();
            if (config == null) return 3.00m;

            return Math.Max(config.ComissaoBase, taxaEntrega * 0.5m); // 50% da taxa ou valor mínimo
        }

        private async Task<Entrega> ObterEntregaCompleta(int entregaId)
        {
            return await _context.Entregas
                .Include(e => e.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Include(e => e.Entregador)
                .Include(e => e.Historico)
                .FirstOrDefaultAsync(e => e.Id == entregaId);
        }

        private async Task AdicionarHistoricoEntrega(int entregaId, string statusAnterior, string statusNovo, string usuario, string? observacao = null)
        {
            var historico = new HistoricoEntrega
            {
                EntregaId = entregaId,
                StatusAnterior = statusAnterior,
                StatusNovo = statusNovo,
                Observacao = observacao,
                UsuarioId = 1, // Temporário
                UsuarioNome = usuario,
                DataAlteracao = DateTime.UtcNow
            };

            _context.HistoricoEntregas.Add(historico);
            await _context.SaveChangesAsync();
        }
    }
}