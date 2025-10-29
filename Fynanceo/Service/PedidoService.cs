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
            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido == null)
                throw new ArgumentException("Pedido não encontrado");

            var statusAnterior = pedido.Status.ToString();

            if (Enum.TryParse<PedidoStatus>(novoStatus, out var status))
            {
                pedido.Status = status;

                // Atualizar timestamps
                switch (status)
                {
                    case PedidoStatus.EnviadoCozinha:
                        pedido.DataEnvioCozinha = DateTime.UtcNow;
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

                await _context.SaveChangesAsync();
                await AdicionarHistorico(pedidoId, statusAnterior, novoStatus, usuario);
            }

            return await ObterPedidoCompleto(pedidoId);
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
            if (Enum.TryParse<PedidoStatus>(status, out var statusEnum))
            {
                var pedidos = await _context.Pedidos
     .Where(p => p.Status == statusEnum)
     .OrderByDescending(p => p.DataAbertura)
     .Select(p => new
     {
         p.Id,
         p.Status,
         Itens = p.Itens.Select(i => new
         {
             i.Quantidade,
             ProdutoNome = i.Produto.Nome
         })
     })
     .ToListAsync();

                //return await _context.Pedidos
                //      .Include(p => p.Mesa)
                //      .Include(p => p.Cliente)
                //      .Include(p => p.Itens)
                //          .ThenInclude(i => i.Produto) // 👈 Inclui o produto de cada item
                //      .Where(p => p.Status == statusEnum)
                //      .OrderByDescending(p => p.DataAbertura)
                //      .ToListAsync();
            }
    

            return new List<Pedido>();
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
            var item = await _context.ItensPedido.FindAsync(itemId);
            if (item == null)
                return null;

            item.EmPreparo = true;
            item.EnviadoCozinha = true;
            item.DataInicioPreparo = DateTime.Now;

            await _context.SaveChangesAsync();
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
        public async Task<bool> IniciarPreparoTodosAsync(int pedidoId)
        {
            var itens = await _context.ItensPedido
                .Where(i => i.PedidoId == pedidoId && !i.EmPreparo && !i.Pronto)
                .ToListAsync();

            if (!itens.Any())
                return false;

            foreach (var item in itens)
            {
                item.EmPreparo = true;
                item.EnviadoCozinha = true;
                item.DataInicioPreparo = DateTime.Now;
            }

            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido != null && pedido.Status == PedidoStatus.EnviadoCozinha)
            {
                pedido.Status = PedidoStatus.EmPreparo;
                pedido.DataPreparo = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
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
