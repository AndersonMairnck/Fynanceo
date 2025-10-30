// Services/PedidoService.cs
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.ViewModels;
using Fynanceo.Models.Enums;



namespace Fynanceo.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly AppDbContext _context;

        public PedidoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CriarPedido(PedidoViewModel viewModel)
        {
            try
            {
                var pedido = new Pedido
                {

                    NumeroPedido = viewModel.TipoPedido.ToString().Substring(0, 1).ToUpper() + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    TipoPedido = viewModel.TipoPedido,
                    Status = PedidoStatus.Aberto,
                    MesaId = viewModel.MesaId,
                    ClienteId = viewModel.ClienteId,
                    EnderecoEntregaId = viewModel.EnderecoEntregaId,
                    Observacoes = viewModel.Observacoes,
                    TaxaEntrega = viewModel.TaxaEntrega,
                    DataAbertura = DateTime.Now,
                    FuncionarioId = 4 // Temporário - depois pegar do usuário logado
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Adicionar itens se houver
                if (viewModel.Itens.Any())
                {

                    foreach (var item in viewModel.Itens)
                    {
                        await AdicionarItem(pedido.Id, item);
                    }
                    await RecalcularTotais(pedido.Id);
                }
                // Adicionar histórico
                await AdicionarHistorico(pedido.Id, "Novo", pedido.Status.ToString(), "Sistema");

                return pedido;
            }
            catch (DbUpdateException ex)
            {
                // Aqui você pode logar o erro ou exibir para o usuário
                throw new Exception("Erro ao salvar o pedido: " + ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<Pedido> AdicionarItem(int pedidoId, ItemPedidoViewModel itemVm)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new ArgumentException("Pedido não encontrado");

            var produto = await _context.Produtos.FindAsync(itemVm.ProdutoId);
            if (produto == null)
                throw new ArgumentException("Produto não encontrado");

            var itemPedido = new ItemPedido
            {
                PedidoId = pedidoId,
                ProdutoId = itemVm.ProdutoId,
                Quantidade = itemVm.Quantidade,
                PrecoUnitario = produto.ValorVenda,
                Observacoes = itemVm.Observacoes,
                Personalizacoes = itemVm.Personalizacoes
            };

            _context.ItensPedido.Add(itemPedido);
            await _context.SaveChangesAsync();

            // Recalcular totais
            // await RecalcularTotais(pedidoId);

            return await ObterPedidoCompleto(pedidoId);
        }

        public async Task<bool> RemoverItem(int itemPedidoId)
        {
            var item = await _context.ItensPedido.FindAsync(itemPedidoId);
            if (item == null) return false;

            _context.ItensPedido.Remove(item);
            await _context.SaveChangesAsync();

            await RecalcularTotais(item.PedidoId);
            return true;
        }

        public async Task<Pedido> AtualizarStatus(int pedidoId, string novoStatus, string usuario)
        {
            // Inicia uma transação no banco — tudo dentro dela só será confirmado no Commit()
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Busca o pedido no banco de dados
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                    throw new ArgumentException("Pedido não encontrado");

                // Guarda o status anterior para registrar no histórico
                var statusAnterior = pedido.Status.ToString();

                // Tenta converter o novoStatus (string) para o enum PedidoStatus
                if (Enum.TryParse<PedidoStatus>(novoStatus, out var status))
                {
                    pedido.Status = status;

                    // Atualiza campos de data conforme o novo status
                    switch (status)
                    {
                        case PedidoStatus.EnviadoCozinha:
                            pedido.DataEnvioCozinha = DateTime.UtcNow;

                            // 🔹 Atualiza todos os itens do pedido
                            var itens = await _context.ItensPedido
                                .Where(i => i.PedidoId == pedidoId)
                                .ToListAsync();

                            foreach (var item in itens)
                            {
                                item.EnviadoCozinha = true;
                                item.DataEnvioCozinha = DateTime.UtcNow;
                            }

                            // Marca os itens para atualização
                            _context.ItensPedido.UpdateRange(itens);
                            break;

                        case PedidoStatus.EmPreparo:
                            pedido.DataPreparo = DateTime.Now;
                            break;

                        case PedidoStatus.Pronto:
                            pedido.DataPronto = DateTime.Now;
                            break;

                        case PedidoStatus.Entregue:
                            pedido.DataEntrega = DateTime.Now;
                            break;

                        case PedidoStatus.Fechado:
                            pedido.DataFechamento = DateTime.Now;
                            break;
                    }

                    // 🔸 Salva todas as alterações (pedido + itens)
                    await _context.SaveChangesAsync();

                    // 🔸 Registra o histórico de alteração
                    await AdicionarHistorico(pedidoId, statusAnterior, novoStatus, usuario);

                    // 🔸 Confirma a transação — grava tudo de uma vez
                    await transaction.CommitAsync();
                }

                // Retorna o pedido completo e atualizado
                return await ObterPedidoCompleto(pedidoId);
            }
            catch (Exception ex)
            {
                // Se ocorrer erro, desfaz tudo que foi feito até aqui
                await transaction.RollbackAsync();

                // Lança uma nova exceção explicando o que deu errado
                throw new Exception($"Falha ao atualizar o status do pedido {pedidoId}: {ex.Message}", ex);
            }
        }

        public async Task<Pedido> ObterPedidoCompleto(int pedidoId)
        {
            return await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Cliente)
                .Include(p => p.EnderecoEntrega)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .Include(p => p.Historico)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);
        }

        public async Task<List<Pedido>> ObterPedidosPorStatus(string status)
        {
            //if (Enum.TryParse<PedidoStatus>(status, out var statusEnum))
            //{
            //    return await _context.Pedidos
            //        .Include(p => p.Itens)
            //            .ThenInclude(i => i.Produto)
            //        .Include(p => p.Mesa)
            //        .Where(p => p.Status == statusEnum)
            //        .OrderByDescending(p => p.DataAbertura)
            //        .ToListAsync();
            //}


            var query = _context.Pedidos
        .Include(p => p.Itens)
            .ThenInclude(i => i.Produto)
        .Include(p => p.Mesa)
        .AsQueryable();

            switch (status)
            {
                case "EnviadoCozinha":
                    // Itens que foram enviados, mas ainda não estão em preparo nem prontos
                    query = query.Where(p => p.Itens.Any(i =>
                        i.EnviadoCozinha && !i.EmPreparo && !i.Pronto));
                    break;

                case "EmPreparo":
                    // Itens que já estão em preparo, mas ainda não prontos
                    query = query.Where(p => p.Itens.Any(i =>
                        i.EnviadoCozinha && i.EmPreparo && !i.Pronto));
                    break;

                case "Pronto":
                    // Itens que estão totalmente prontos
                    query = query.Where(p => p.Itens.Any(i =>
                        i.EnviadoCozinha && i.EmPreparo && i.Pronto));
                    break;

                default:
                    return new List<Pedido>();
            }

            return await query
                .OrderByDescending(p => p.DataAbertura)
                .ToListAsync();
        }

        public async Task<List<Pedido>> ObterPedidosDoDia()
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);
            return await _context.Pedidos
                 .Include(p => p.Mesa)
                 .Include(p => p.Cliente)
                 .Include(p => p.Itens)
                 .Where(p => p.DataAbertura >= hoje && p.DataAbertura < amanha)
                .OrderByDescending(p => p.DataAbertura)
                .ToListAsync();
        }

        public async Task<decimal> CalcularTotalPedido(int pedidoId)
        {
            var itens = await _context.ItensPedido
                .Where(i => i.PedidoId == pedidoId)
                .ToListAsync();

            var subtotal = itens.Sum(i => i.Total);
            var pedido = await _context.Pedidos.FindAsync(pedidoId);

            return subtotal + (pedido?.TaxaEntrega ?? 0);
        }

        private async Task RecalcularTotais(int pedidoId)
        {
            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido != null)
            {
                try
                {
                    //pedido.Subtotal = await _context.ItensPedido
                    //    .Where(i => i.PedidoId == pedidoId)
                    //    .SumAsync(i => i.Total);

                    var itens = await _context.ItensPedido
        .Where(i => i.PedidoId == pedidoId)
        .ToListAsync();

                    pedido.Subtotal = itens.Sum(i => i.Quantidade * i.PrecoUnitario);




                    pedido.Total = pedido.Subtotal + pedido.TaxaEntrega;

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Captura qualquer erro, inclusive de consulta
                    throw new Exception("Erro ao recalcular totais: " + ex.InnerException?.Message ?? ex.Message);
                }
            }
        }

        private async Task AdicionarHistorico(int pedidoId, string statusAnterior, string statusNovo, string usuario)
        {
            var historico = new HistoricoPedido
            {
                PedidoId = pedidoId,
                StatusAnterior = statusAnterior,
                StatusNovo = statusNovo,
                UsuarioId = 1, // Temporário
                UsuarioNome = usuario,
                DataAlteracao = DateTime.Now
            };

            _context.HistoricoPedido.Add(historico);
            await _context.SaveChangesAsync();
        }
        // 🔹 Iniciar preparo de um item específico
        public async Task<ItemPedido?> IniciarPreparoItemAsync(int itemId)
        {
            // 1️⃣ Busca o item do pedido
            var item = await _context.ItensPedido.FindAsync(itemId);
            if (item == null)
                return null;

            // Evita reprocessar item já em preparo
            if (item.EmPreparo)
                throw new InvalidOperationException("O item já está em preparo.");

            // 2️⃣ Atualiza o estado do item
            item.EmPreparo = true;
            item.EnviadoCozinha = true;
            item.DataInicioPreparo = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 3️⃣ Verifica se ainda existem outros itens do mesmo pedido não enviados
            // 3️⃣ Verifica se ainda existem outros itens do mesmo pedido não enviados
            bool existeOutroNaoEnviado = await _context.ItensPedido
                .AnyAsync(i => i.PedidoId == item.PedidoId && i.EmPreparo == false);

            // 4️⃣ Se não existir, muda o status do pedido para "Em Preparo"
            if (!existeOutroNaoEnviado)
            {
                var pedido = await _context.Pedidos.FindAsync(item.PedidoId);
                if (pedido != null)
                {
                    pedido.Status = PedidoStatus.EmPreparo; // Ajuste conforme seu enum
                    pedido.DataPreparo = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return item;
        }





        // 🔹 Marcar um item como pronto e atualizar o status do pedido se necessário
        public async Task<ItemPedido?> MarcarProntoItemAsync(int itemId)
        {
            var item = await _context.ItensPedido.FindAsync(itemId);
            if (item == null)
                return null;

            item.Pronto = true;
            item.EmPreparo = false;
            item.DataPronto = DateTime.Now;

            // Atualiza o status do pedido, se todos os itens estiverem prontos
            var pedido = await _context.Pedidos.FindAsync(item.PedidoId);
            if (pedido != null)
            {
                var itensPendentes = await _context.ItensPedido
                    .Where(i => i.PedidoId == item.PedidoId && !i.Pronto)
                    .AnyAsync();

                if (!itensPendentes)
                {
                    pedido.Status = PedidoStatus.Pronto;
                    pedido.DataPronto = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return item;
        }

        // 🔹 Iniciar preparo de todos os itens de um pedido

        /// <summary>
        /// Inicia o preparo de todos os itens de um pedido e atualiza o status do pedido.
        /// </summary>
        /// <param name="pedidoId">ID do pedido</param>
        /// <returns>Retorna true se algum item foi iniciado, false se não houver itens disponíveis</returns>
        public async Task<bool> IniciarPreparoTodosAsync(int pedidoId)
        {
            // 🔹 Inicia uma transação para garantir que a atualização de itens e do pedido seja atômica
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🔹 Busca todos os itens do pedido que ainda não estão em preparo nem prontos
                var itens = await _context.ItensPedido
                    .Where(i => i.PedidoId == pedidoId && !i.EmPreparo && !i.Pronto)
                    .ToListAsync();

                // 🔹 Se não houver itens disponíveis, retorna false
                if (!itens.Any())
                    return false;

                // 🔹 Atualiza cada item para indicar que está em preparo
                foreach (var item in itens)
                {
                    item.EmPreparo = true;                     // Marca como em preparo
                    item.EnviadoCozinha = true;                // Marca como enviado para a cozinha
                    item.DataInicioPreparo = DateTime.UtcNow;     // Registra o horário de início do preparo

                    // 🔹 Se ainda não tiver data de envio para cozinha, define agora
                    item.DataEnvioCozinha ??= DateTime.UtcNow;
                }

                // 🔹 Busca o pedido correspondente
                var pedido = await _context.Pedidos.FindAsync(pedidoId);

                // 🔹 Atualiza o status do pedido se ainda estiver "EnviadoCozinha"
                if (pedido != null && pedido.Status == PedidoStatus.EnviadoCozinha)
                {
                    pedido.Status = PedidoStatus.EmPreparo;     // Atualiza o status do pedido
                    pedido.DataPreparo = DateTime.UtcNow;          // Registra a data/hora de início do preparo do pedido
                }

                // 🔹 Persiste todas as alterações no banco
                await _context.SaveChangesAsync();

                // 🔹 Confirma a transação
                await transaction.CommitAsync();

                // 🔹 Retorna true indicando que os itens foram iniciados
                return true;
            }
            catch (Exception)
            {
                // 🔹 Em caso de erro, desfaz todas as alterações da transação
                await transaction.RollbackAsync();
                throw; // Propaga a exceção para o controller tratar
            }
        }



        // 🔹 Marcar todos os itens como prontos e atualizar o status do pedido
        public async Task<bool> MarcarProntoTodosAsync(int pedidoId)
        {
            var itens = await _context.ItensPedido
                .Where(i => i.PedidoId == pedidoId && !i.Pronto)
                .ToListAsync();

            if (!itens.Any())
                return false;

            foreach (var item in itens)
            {
                item.Pronto = true;
                item.EmPreparo = false;
                item.DataPronto = DateTime.Now;
            }

            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido != null)
            {
                pedido.Status = PedidoStatus.Pronto;
                pedido.DataPronto = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }




}
