// Services/PedidoService.cs

using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.EstoquesModel;
using Fynanceo.ViewModel.PedidosModel;
using Fynanceo.ViewModel.FinanceirosModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Diagnostics;


namespace Fynanceo.Service
{
    public class PedidoService : IPedidoService
    {
        private readonly AppDbContext _context;
        private readonly IMesaService _mesaService;
        private readonly IEstoqueService _estoqueService;
   
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<UsuarioAplicacao> _userManager;

        public PedidoService(AppDbContext context, 
                   IMesaService mesaService, 
                           IServiceProvider serviceProvider,
                                   IEstoqueService estoqueService,  
                                            IHttpContextAccessor httpContextAccessor,
                                                        UserManager<UsuarioAplicacao> userManager)
        {
            _context = context;
            _mesaService = mesaService;
            //  _entregaService = entregaService;
            _serviceProvider = serviceProvider;
            _estoqueService = estoqueService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
           
        }

        private async Task<string> GetCurrentUserNameAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            return user?.UserName ?? "Sistema";
        }

        private async Task<(bool Disponivel, string Mensagem)> ValidarEstoqueProdutoAsync(int produtoId, decimal quantidadeSolicitada)
        {
            var produto = await _context.Produtos
                .Include(p => p.ProdutoIngredientes)
                .ThenInclude(pi => pi.Estoque)
                .FirstOrDefaultAsync(p => p.Id == produtoId);

            if (produto == null) return (false, "Produto não encontrado.");

            // 1. Verificar estoque direto
            if (produto.IdEstoque > 0)
            {
                var estoque = await _context.Estoques.FindAsync(produto.IdEstoque);
                if (estoque != null && estoque.EstoqueAtual < quantidadeSolicitada)
                {
                    return (false, $"Estoque insuficiente para o produto {produto.Nome}. Disponível: {estoque.EstoqueAtual:N3}, Solicitado: {quantidadeSolicitada:N3}");
                }
            }

            // 2. Verificar ingredientes
            if (produto.ProdutoIngredientes != null && produto.ProdutoIngredientes.Any())
            {
                foreach (var ingrediente in produto.ProdutoIngredientes)
                {
                    var qtdNecessaria = ingrediente.Quantidade * quantidadeSolicitada;
                    var estoqueIngrediente = await _context.Estoques.FindAsync(ingrediente.EstoqueId);

                    if (estoqueIngrediente != null && estoqueIngrediente.EstoqueAtual < qtdNecessaria)
                    {
                        var qtdPossivel = ingrediente.Quantidade > 0 ? estoqueIngrediente.EstoqueAtual / ingrediente.Quantidade : 0;
                        return (false, $"Estoque insuficiente do ingrediente {estoqueIngrediente.Nome} para o produto {produto.Nome}. Disponível: {estoqueIngrediente.EstoqueAtual:N3}, Necessário: {qtdNecessaria:N3}. Máximo possível: {qtdPossivel:N3}");
                    }
                }
            }

            return (true, string.Empty);
        }

        public async Task<Pedido> FecharPedidoAsync(int pedidoId)
        {
            var usuarioNome = await GetCurrentUserNameAsync();

            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Include(p => p.Mesa)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new ArgumentException("Pedido não encontrado");

            // Verifica se o pedido já está fechado ou cancelado
            if (pedido.Status == PedidoStatus.Fechado || pedido.Status == PedidoStatus.Cancelado)
            {
                throw new InvalidOperationException("Pedido já está " + pedido.Status);
            }


            // Verifica se todos os itens estão entregues ou cancelados
            var itensPendentes = pedido.Itens.Where(i =>
                i.Status != PedidoStatus.Entregue &&
                i.Status != PedidoStatus.Cancelado
            ).ToList();

            if (pedido.TipoPedido != TipoPedido.Balcao)
            {
                if (itensPendentes.Any())
                {
                    var itensPendentesNomes = string.Join(", ", itensPendentes.Select(i => i.Produto.Nome));
                    throw new InvalidOperationException(
                        $"Não é possível fechar o pedido. Itens pendentes: {itensPendentesNomes}");
                }
            }

            // Fecha o pedido
            pedido.Status = PedidoStatus.Fechado;
            pedido.DataFechamento = DateTime.UtcNow;

            // Libera a mesa
            if (pedido.Mesa != null)
            {
                pedido.Mesa.Status = "Livre";
            }

            await _context.SaveChangesAsync();

            // Adiciona histórico
            await AdicionarHistorico(pedidoId, pedido.Status.ToString(), "Fechado", usuarioNome);

            return pedido;
        }

        public async Task<Pedido> CriarPedido(PedidoViewModel viewModel)
        {
            var usuarioNome = await GetCurrentUserNameAsync();
            try
            {
                // Validar estoque para todos os itens antes de criar o pedido
                if (viewModel.Itens != null && viewModel.Itens.Any())
                {
                    foreach (var item in viewModel.Itens)
                    {
                        var (disponivel, mensagem) = await ValidarEstoqueProdutoAsync(item.ProdutoId, item.Quantidade);
                        if (!disponivel)
                        {
                            throw new InvalidOperationException(mensagem);
                        }
                    }
                }

                var pedido = new Pedido
                {
                    NumeroPedido = viewModel.TipoPedido.ToString().Substring(0, 1).ToUpper() +
                                   DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                    TipoPedido = viewModel.TipoPedido,
                    Status = PedidoStatus.Aberto,
                    MesaId = viewModel.MesaId,
                    ClienteId = viewModel.ClienteId,
                    EnderecoEntregaId = viewModel.EnderecoEntregaId,
                    Observacoes = viewModel.Observacoes,
                    TaxaEntrega = viewModel.TaxaEntrega,
                    DataAbertura = DateTime.UtcNow,
                    UsuarioNome = usuarioNome, 
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
                await AdicionarHistorico(pedido.Id, "Novo", pedido.Status.ToString(), usuarioNome);

                return pedido;
            }
            catch (DbUpdateException ex)
            {
                // Aqui você pode logar o erro ou exibir para o usuário
                var mensagem = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Erro ao salvar o pedido: {mensagem}");
            }
        }

        public async Task<Pedido> AdicionarItem(int pedidoId, ItemPedidoViewModel itemVm)
        {
            var usuarioNome = await GetCurrentUserNameAsync();
            
            var (disponivel, mensagem) = await ValidarEstoqueProdutoAsync(itemVm.ProdutoId, itemVm.Quantidade);
            if (!disponivel)
            {
                throw new InvalidOperationException(mensagem);
            }

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
                Personalizacoes = itemVm.Personalizacoes,
                IdEstoque = produto.IdEstoque,


                //Total = itemVm.Quantidade * produto.ValorVenda // Calcula o total do item
            };
            // if (pedido.TipoPedido == TipoPedido.Delivery && produto.TempoPreparoMinutos == 0)
            //     itemPedido.Status = PedidoStatus.Pronto;
            // if (pedido.TipoPedido == TipoPedido.Balcao && produto.TempoPreparoMinutos == 0)
            //     itemPedido.Status = PedidoStatus.Pronto;
            
            if (produto.TempoPreparoMinutos == 0)
                itemPedido.Status = PedidoStatus.Pronto;
            
            _context.ItensPedido.Add(itemPedido);
            await _context.SaveChangesAsync();

            if (produto.IdEstoque > 0 && itemPedido.Status == PedidoStatus.Pronto)
            { 
                    await _estoqueService.CriarMovimentacaoAsync(new MovimentacaoEstoqueViewModel
                    {
                        Tipo = TipoMovimentacaoEstoque.Saida,
                        EstoqueId = produto.IdEstoque,
                        Quantidade = itemPedido.Quantidade,
                        CustoUnitario = produto.CustoUnitario,
                        Observacao = $"Material usado para o pedido {itemPedido.PedidoId}",
                        Documento =  $"{pedido.TipoPedido}: P= {itemPedido.PedidoId}",
                        FornecedorId = 0,
                        PedidoId = itemPedido.PedidoId,
                        UsuarioNome = usuarioNome,
                    });
                }
                      
            
            // Recalcular totais
            await RecalcularTotais(pedidoId);

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

        public async Task<Pedido> AtualizarStatus(int pedidoId, string novoStatus)
        {
       

            var usuario = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        

            try
            {
                // 🔹 Busca o pedido no banco de dados
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                    throw new ArgumentException("Pedido não encontrado.");

                // 🔹 Guarda o status anterior para histórico
                var statusAnterior = pedido.Status.ToString();

                // 🔹 Converte string para enum PedidoStatus
                if (Enum.TryParse<PedidoStatus>(novoStatus, out var status))
                {
                    pedido.Status = status;

                    //🔹 Busca todos os itens do pedido (para sincronizar o status)
                    var itens = await _context.ItensPedido
                        .Where(i => i.PedidoId == pedidoId &&
                                    i.Produto.TempoPreparoMinutos > 0) // Apenas itens que precisam de preparo)
                        .ToListAsync();


                    // 🔹 Atualiza campos conforme o novo status
                    switch (status)
                    {
                        case PedidoStatus.EnviadoCozinha:
                            pedido.DataEnvioCozinha = DateTime.UtcNow;

                            foreach (var item in itens)
                            {
                                item.Status = PedidoStatus.EnviadoCozinha;
                                item.DataEnvioCozinha = DateTime.UtcNow;
                            }

                            break;

                        case PedidoStatus.EmPreparo:
                            pedido.DataPreparo = DateTime.UtcNow;

                            foreach (var item in itens.Where(i => i.Status != PedidoStatus.Pronto))
                            {
                                item.Status = PedidoStatus.EmPreparo;
                                item.DataInicioPreparo = DateTime.UtcNow;
                            }

                            break;

                        case PedidoStatus.Pronto:
                            pedido.DataPronto = DateTime.UtcNow;

                            foreach (var item in itens)
                            {
                                item.Status = PedidoStatus.Pronto;
                                item.DataPronto = DateTime.UtcNow;
                            }

                            break;

                        case PedidoStatus.Entregue:
                            pedido.DataEntrega = DateTime.UtcNow;
                            break;

                        case PedidoStatus.Fechado:
                            pedido.DataFechamento = DateTime.UtcNow;
                            break;
                    }

                    // 🔹 Atualiza os itens no contexto
                    _context.ItensPedido.UpdateRange(itens);
                    
                    // 🔹 Salva todas as alterações (pedido + itens)
                    await _context.SaveChangesAsync();
                    
                    
                    
               
                    

                    // 🔹 Registra histórico
                    await AdicionarHistorico(pedidoId, statusAnterior, novoStatus, usuario.UserName);

                  
                }

                // 🔹 Retorna o pedido completo e atualizado
                return await ObterPedidoCompleto(pedidoId);
            }
            catch (Exception ex)
            {
             
                throw new Exception($"Falha ao atualizar o status do pedido {pedidoId}: {ex.Message}", ex);
            }
        }

        public async Task<Pedido> ObterPedidoCompleto(int pedidoId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Cliente)
                .Include(p => p.EnderecoEntrega)
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Include(p => p.Historico)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new KeyNotFoundException($"Pedido {pedidoId} não encontrado.");

            return pedido;
        }

        public async Task<List<Pedido>> ObterPedidosPorStatus(string status)
        {
            if (Enum.TryParse<PedidoStatus>(status, out var statusEnum))
            {
                return await _context.Pedidos
                    .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                    .Include(p => p.Mesa)
                    .Where(p => p.Itens.Any(i => i.Status == statusEnum))
                    .OrderByDescending(p => p.DataAbertura)
                    .ToListAsync();
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
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido != null)
            {
                try
                {
                    // Considera apenas itens não cancelados
                    var itensNaoCancelados = pedido.Itens.Where(i => i.Status != PedidoStatus.Cancelado).ToList();

                    pedido.Subtotal = itensNaoCancelados.Sum(i => i.Quantidade * i.PrecoUnitario);
                    pedido.Total = pedido.Subtotal + pedido.TaxaEntrega;

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var mensagem = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Erro ao salvar o pedido: {mensagem}");
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
                //UsuarioId = 1, // Temporário
                UsuarioNome = usuario,
                DataAlteracao = DateTime.UtcNow
            };

            _context.HistoricoPedido.Add(historico);
            await _context.SaveChangesAsync();
        }

        // 🔹 Iniciar preparo de um item específico
        public async Task<ItemPedido?> IniciarPreparoItemAsync(int itemId)
        {
            var usuario = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            // 1️⃣ Busca o item do pedido no banco
            var item = await _context.ItensPedido.FindAsync(itemId);
            if (item == null)
                return null;

            // 2️⃣ Impede reprocessar item já em preparo ou pronto
            if (item.Status == PedidoStatus.EmPreparo || item.Status == PedidoStatus.Pronto)
                throw new InvalidOperationException("O item já está em preparo ou pronto.");

            // 3️⃣ Atualiza o status do item
            item.Status = PedidoStatus.EmPreparo;
            item.DataInicioPreparo = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 4️⃣ Verifica se todos os itens do pedido já estão em preparo
            bool todosEmPreparo = await _context.ItensPedido
                .Where(i => i.PedidoId == item.PedidoId)
                .AllAsync(i => i.Status == PedidoStatus.EmPreparo || i.Status == PedidoStatus.Pronto);

            // 5️⃣ Se todos estão em preparo ou prontos, atualiza o status do pedido
            if (todosEmPreparo)
            {
                var pedido = await _context.Pedidos.FindAsync(item.PedidoId);
                if (pedido != null)
                {
                    // AtualizarStatus(pedido.Id, PedidoStatus.EmPreparo.ToString(), "Sistema").Wait();
                    await AtualizarStatus(pedido.Id, nameof(PedidoStatus.EmPreparo));
                }
            }

            return item;
        }

        // 🔹 Marcar um item como pronto e atualizar o status do pedido se necessário
        public async Task<ItemPedido?> MarcarProntoItemAsync(int itemId)
        {
            var usuario = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
  // 1️⃣ Busca o item do pedido
            var item = await _context.ItensPedido.FindAsync(itemId);
            if (item == null)
                return null;

            // 2️⃣ Impede marcar como pronto se já estiver pronto
            if (item.Status == PedidoStatus.Pronto)
                throw new InvalidOperationException("O item já está marcado como pronto.");

            // 3️⃣ Atualiza o status do item para pronto e a data
            item.Status = PedidoStatus.Pronto;
            item.DataPronto = DateTime.UtcNow;

            await _context.SaveChangesAsync();
      
             var produtoUsados =  await _context.Produtos
                .Include(p => p.ProdutoIngredientes)
                .ThenInclude(i => i.Estoque)
                .FirstOrDefaultAsync(p => p.Id == item.ProdutoId);
             
            foreach (var materiais in produtoUsados.ProdutoIngredientes)
            {
                await _estoqueService.CriarMovimentacaoAsync(new MovimentacaoEstoqueViewModel
                {
                    Tipo = TipoMovimentacaoEstoque.Saida,
                    EstoqueId = materiais.EstoqueId,
                    Quantidade = materiais.Quantidade,
                    CustoUnitario = materiais.Estoque.CustoUnitario,
                    Observacao = $"Material usado para o pedido {item.PedidoId}",
                    Documento =  $"Pedido {item.PedidoId}",
                    FornecedorId = materiais.Estoque.FornecedorId,
                    PedidoId = item.PedidoId,
                    UsuarioNome = usuario.UserName
                });
            }


            // 4️⃣ Verifica se todos os itens do mesmo pedido estão prontos
            bool todosProntos = await _context.ItensPedido
                .Where(i => i.PedidoId == item.PedidoId)
                .AllAsync(i => i.Status == PedidoStatus.Pronto);

            // 5️⃣ Se todos estiverem prontos, atualiza o status do pedido
            if (todosProntos)
            {
                var pedido = await _context.Pedidos.FindAsync(item.PedidoId);

                if (pedido != null)
                {
                    // 🔥 SEM Wait() e sem bloquear thread!
                    // await AtualizarStatus(pedido.Id, PedidoStatus.Pronto.ToString(), "Sistema");
                  await  AtualizarStatus(pedido.Id, nameof(PedidoStatus.Pronto));

                    await _context.Entry(pedido).ReloadAsync();

                    // 🔥 Somente depois do await, o pedido está salvo e atualizado
                    if (pedido is { TipoPedido: TipoPedido.Delivery, Status: PedidoStatus.Pronto })
                    {
                        // ara evitar conflitos entre o entregaservice e o pedidoservice ou invez de colocar
                        //  no inicio    private readonly IEntregaService _entregaService; ele so inicia neste momento
                        // para nao quebrar na hora de executar?
                        var entregaService = _serviceProvider.GetRequiredService<IEntregaService>();
                        await entregaService.CriarEntrega(pedido.Id);
                    }
                }
            }
            await transaction.CommitAsync();
            return item;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await transaction.RollbackAsync();
                throw;
            }
          
        }

        /// Inicia o preparo de todos os itens de um pedido e atualiza o status do pedido.
        // <returns>Retorna true se algum item foi iniciado, false se não houver itens disponíveis</returns>
        public async Task<bool> IniciarPreparoTodosAsync(int pedidoId)
        {
            // 🔹 Inicia uma transação para garantir atomicidade
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🔹 Busca todos os itens do pedido que ainda não estão em preparo nem prontos
                var itens = await _context.ItensPedido
                    .Where(i => i.PedidoId == pedidoId && i.Status == PedidoStatus.EnviadoCozinha)
                    .ToListAsync();

                // 🔹 Se não houver itens disponíveis, retorna false
                if (!itens.Any())
                    return false;

                // 🔹 Atualiza cada item para indicar que está em preparo
                foreach (var item in itens)
                {
                    item.Status = PedidoStatus.EmPreparo;
                    item.DataInicioPreparo = DateTime.UtcNow;

                    // Se ainda não tiver data de envio para cozinha, define agora
                    item.DataEnvioCozinha ??= DateTime.UtcNow;
                }

                // 🔹 Busca o pedido correspondente
                var pedido = await _context.Pedidos.FindAsync(pedidoId);

                // 🔹 Atualiza o status do pedido, se aplicável /O pedido != null é automaticamente tratado pelo pattern.
                if (pedido is { Status: PedidoStatus.EnviadoCozinha })
                {
                    //  AtualizarStatus(pedido.Id, PedidoStatus.EmPreparo.ToString(), "Sistema").Wait();
                    await   AtualizarStatus(pedido.Id, nameof(PedidoStatus.EmPreparo));
                    //pedido.Status = PedidoStatus.EmPreparo;
                    //pedido.DataPreparo = DateTime.UtcNow;
                }

                // 🔹 Persiste todas as alterações no banco
                await _context.SaveChangesAsync();

                // 🔹 Confirma a transação
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                // 🔹 Em caso de erro, desfaz a transação
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> EntregaTodosCozinha(int pedidoId)
        {
            // 🔹 Inicia uma transação para garantir atomicidade
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🔹 Busca todos os itens do pedido que ainda não estão em preparo nem prontos
                var itens = await _context.ItensPedido
                    .Where(i => i.PedidoId == pedidoId && i.Status == PedidoStatus.Pronto)
                    .ToListAsync();

                // 🔹 Se não houver itens disponíveis, retorna false
                if (!itens.Any())
                    return false;

                // 🔹 Atualiza cada item para indicar que está entregue
                foreach (var item in itens)
                {
                    item.Status = PedidoStatus.Entregue;
                    item.DataEntrega = DateTime.UtcNow;

                    // Se ainda não tiver data de envio para cozinha, define agora
                    item.DataEnvioCozinha ??= DateTime.UtcNow;
                }

                // 🔹 Busca o pedido correspondente
                var pedido = await _context.Pedidos.FindAsync(pedidoId);

                // 🔹 Atualiza o status do pedido, se aplicável

                if (pedido is { Status: PedidoStatus.Pronto })
                {
                    //AtualizarStatus(pedido.Id, PedidoStatus.EmPreparo.ToString(), "Sistema").Wait();
                    await   AtualizarStatus(pedido.Id, nameof(PedidoStatus.EmPreparo));
                    //pedido.Status = PedidoStatus.EmPreparo;
                    //pedido.DataPreparo = DateTime.UtcNow;
                }

                // 🔹 Persiste todas as alterações no banco
                await _context.SaveChangesAsync();

                // 🔹 Confirma a transação
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                // 🔹 Em caso de erro, desfaz a transação
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 🔹 Marcar todos os itens como prontos e atualizar o status do pedido
        public async Task<bool> MarcarProntoTodosAsync(int pedidoId)
        {
            // 🔹 Inicia uma transação para garantir atomicidade
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Busca todos os itens do pedido que ainda não estão prontos
                var itens = await _context.ItensPedido
                    .Where(i => i.PedidoId == pedidoId && i.Status != PedidoStatus.Pronto)
                    .ToListAsync();

                // Se não houver itens pendentes, retorna falso
                if (!itens.Any())
                    return false;

                // Atualiza cada item para o status "Pronto"
                foreach (var item in itens)
                {
                    item.Status = PedidoStatus.Pronto;
                    item.DataPronto = DateTime.UtcNow;
                }

                // Busca o pedido correspondente
                var pedido = await _context.Pedidos.FindAsync(pedidoId);

                // 🔹 Atualiza o status do pedido, se aplicável

                if (pedido is { Status: PedidoStatus.EmPreparo })
                {
                    //AtualizarStatus(pedido.Id, PedidoStatus.EmPreparo.ToString(), "Sistema").Wait();
                    await AtualizarStatus(pedido.Id, nameof(PedidoStatus.EmPreparo));
                    //pedido.Status = PedidoStatus.EmPreparo;
                    //pedido.DataPreparo = DateTime.UtcNow;
                }

                // Salva todas as alterações
                await _context.SaveChangesAsync();

                // 🔹 Confirma a transação
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                // 🔹 Em caso de erro, desfaz a transação
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ItemPedido?> EntregueIndividualCozinha(int itemId)
        {
            // 1️⃣ Busca o item do pedido no banco
            var item = await _context.ItensPedido.FindAsync(itemId);
            if (item == null)
                return null;

            // 2️⃣ Impede reprocessar item já em preparo ou pronto
            //if (item.Status == PedidoStatus.Pronto || item.Status == PedidoStatus.Entregue)
            //    throw new InvalidOperationException("O item já está em preparo ou entregue.");
            if (item.Status == PedidoStatus.Entregue)
                throw new InvalidOperationException("O item já está em preparo ou entregue.");

            // 3️⃣ Atualiza o status do item
            item.Status = PedidoStatus.Entregue;
            item.DataEntrega = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 4️⃣ Verifica se todos os itens do pedido já estão em preparo
            bool todosEntregue = await _context.ItensPedido
                .Where(i => i.PedidoId == item.PedidoId)
                .AllAsync(i => i.Status == PedidoStatus.Entregue);

            // 5️⃣ Se todos estão em entregue ou prontos, atualiza o status do pedido
            if (todosEntregue)
            {
                var pedido = await _context.Pedidos.FindAsync(item.PedidoId);
                if (pedido != null)
                {
                    // AtualizarStatus(pedido.Id, PedidoStatus.Entregue.ToString(), "Sistema").Wait();
                    await    AtualizarStatus(pedido.Id, nameof(PedidoStatus.Entregue));
                    //pedido.Status = PedidoStatus.EmPreparo; // Enum do pedido
                    //pedido.DataPreparo = DateTime.UtcNow;
                    //await _context.SaveChangesAsync();
                }
            }

            return item;
        }

        public async Task<Pedido> ObterPedidoAtivoPorMesa(int mesaId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                .Where(p => p.MesaId == mesaId &&
                            p.Status != PedidoStatus.Fechado &&
                            p.Status != PedidoStatus.Cancelado)
                .OrderByDescending(p => p.DataAbertura)
                .FirstOrDefaultAsync();

            if (pedido == null)
                throw new InvalidOperationException("Nenhum pedido ativo encontrado para essa mesa.");

            return pedido;
        }

        public async Task<bool> FecharPedidoMesa(int pedidoId, int mesaId)
        {
            var pedido = await _context.Pedidos.FindAsync(pedidoId);
            if (pedido == null || pedido.MesaId != mesaId) return false;

            pedido.Status = PedidoStatus.Fechado;
            pedido.DataFechamento = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Liberar mesa
            await _mesaService.AtualizarStatusAsync(mesaId, "Livre");

            return true;
        }

        public async Task<ItemPedido> EnviarItemCozinhaAsync(int itemId)
        {
            var item = await _context.ItensPedido
                .Include(i => i.Produto)
                .Include(i => i.Pedido)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
                throw new ArgumentException("Item não encontrado");

            // Verifica se é um produto pronto (tempo de preparo = 0)
            if (item.Produto.TempoPreparoMinutos == 0)
            {
                // Produto pronto - marca diretamente como Pronto
                item.Status = PedidoStatus.Pronto;
                item.DataPronto = DateTime.UtcNow;
            }
            else
            {
                // Produto que precisa de preparo - envia para cozinha
                item.Status = PedidoStatus.EnviadoCozinha;
                item.DataEnvioCozinha = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Atualiza status do pedido se necessário
            await AtualizarStatusPedidoAsync(item.PedidoId);

            return item;
        }

        public async Task<ItemPedido> MarcarItemEntregueAsync(int itemId)
        {
            var item = await _context.ItensPedido
                .Include(i => i.Pedido)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
                throw new ArgumentException("Item não encontrado");

            if (item.Status != PedidoStatus.Pronto && item.Status != PedidoStatus.Aberto)
                throw new InvalidOperationException("Item não está pronto para entrega");

            item.Status = PedidoStatus.Entregue;
            item.DataEntrega = DateTime.UtcNow;

            await _context.SaveChangesAsync();
          

            // Atualiza status do pedido se necessário
            await AtualizarStatusPedidoAsync(item.PedidoId);

            return item;
        }

        public async Task<int> EnviarPendentesCozinhaAsync(int pedidoId)
        {
            var itensPendentes = await _context.ItensPedido
                .Include(i => i.Produto)
                .Where(i => i.PedidoId == pedidoId &&
                            i.Status == PedidoStatus.Aberto &&
                            i.Produto.TempoPreparoMinutos > 0) // Apenas itens que precisam de preparo
                .ToListAsync();

            if (!itensPendentes.Any())
                return 0;

            foreach (var item in itensPendentes)
            {
                item.Status = PedidoStatus.EnviadoCozinha;
                item.DataEnvioCozinha = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Atualiza status do pedido
            await AtualizarStatusPedidoAsync(pedidoId);

            return itensPendentes.Count;
        }

        private async Task AtualizarStatusPedidoAsync(int pedidoId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null) return;

            // Verifica se todos os itens estão entregues
            if (pedido.Itens.All(i => i.Status == PedidoStatus.Entregue))
            {
                pedido.Status = PedidoStatus.Entregue;
                pedido.DataEntrega = DateTime.UtcNow;
            }
            // Verifica se todos os itens estão prontos
            else if (pedido.Itens.All(i => i.Status == PedidoStatus.Pronto || i.Status == PedidoStatus.Entregue))
            {
                pedido.Status = PedidoStatus.Pronto;
                pedido.DataPronto = DateTime.UtcNow;
            }
            // Verifica se algum item está em preparo
            else if (pedido.Itens.Any(i => i.Status == PedidoStatus.EmPreparo))
            {
                pedido.Status = PedidoStatus.EmPreparo;
                pedido.DataPreparo = DateTime.UtcNow;
            }
            // Verifica se algum item foi enviado para cozinha
            else if (pedido.Itens.Any(i => i.Status == PedidoStatus.EnviadoCozinha))
            {
                pedido.Status = PedidoStatus.EnviadoCozinha;
                pedido.DataEnvioCozinha = DateTime.UtcNow;
            }
            else if (pedido.TipoPedido == TipoPedido.Delivery)
            {
                pedido.Status = PedidoStatus.EmRota;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Pedido> CancelarPedidoAsync(int pedidoId)
        {
            var usuario = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .Include(p => p.Mesa)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new ArgumentException("Pedido não encontrado");

            // Verifica se o pedido já está cancelado, fechado ou entregue
            if (pedido.Status == PedidoStatus.Cancelado ||
                pedido.Status == PedidoStatus.Fechado ||
                pedido.Status == PedidoStatus.Entregue)
            {
                throw new InvalidOperationException("Não é possível cancelar um pedido com status: " + pedido.Status);
            }

            // Cancela todos os itens do pedido
            foreach (var item in pedido.Itens.Where(i => i.Status != PedidoStatus.Cancelado))
            {
                item.Status = PedidoStatus.Cancelado;
            }

            // Atualiza status do pedido
            pedido.Status = PedidoStatus.Cancelado;
            pedido.DataFechamento = DateTime.UtcNow;

            // Libera a mesa se estiver ocupada
            if (pedido.Mesa is { Status: "Ocupada" })

            {
                pedido.Mesa.Status = "Livre";
            }

            await _context.SaveChangesAsync();

            // Adiciona histórico
            await AdicionarHistorico(pedidoId, pedido.Status.ToString(), "Cancelado", usuario.UserName);

            return pedido;
        }

        public async Task<ItemPedido> CancelarItemAsync(int itemId)
        {
            var item = await _context.ItensPedido
                .Include(i => i.Pedido)
                .Include(i => i.Produto)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
                throw new ArgumentException("Item não encontrado");

            // Verifica se o item já está cancelado, entregue ou em preparo avançado
            if (item.Status == PedidoStatus.Cancelado)
            {
                throw new InvalidOperationException("Item já está cancelado");
            }

            if (item.Status == PedidoStatus.Entregue || item.Status == PedidoStatus.Pronto)
            {
                throw new InvalidOperationException($"Não é possível cancelar um item com status: {item.Status}");
            }

            // Cancela o item
            item.Status = PedidoStatus.Cancelado;

            await _context.SaveChangesAsync();

            // Recalcula totais do pedido
            await RecalcularTotais(item.PedidoId);

            // Atualiza status do pedido
            await AtualizarStatusPedidoAsync(item.PedidoId);

            return item;
        }

        public async Task<bool> VerificaProdutoJaVendido(int produtoId)
        {
            return await _context.ItensPedido
                .AnyAsync(i => i.ProdutoId == produtoId);
        }


        // NO PedidoService.cs - ADICIONAR ESTE MÉTODO
        public async Task<(bool Success, string Message)> FecharPedidoComPagamentoAsync(
            int pedidoId,
            FormaPagamento formaPagamento,
            decimal? valorRecebido = null,
            string? observacoes = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Fechar o pedido
                var pedido = await FecharPedidoAsync(pedidoId);

                // 2. Registrar movimentação no caixa
                var financeiroService = _serviceProvider.GetRequiredService<IFinanceiroService>();
                var caixaAberto = await financeiroService.ObterCaixaAberto();

                if (caixaAberto == null)
                {
                    await transaction.RollbackAsync();
                    return (false, "Nenhum caixa aberto encontrado. Abra um caixa antes de fechar pedidos.");
                }

                // Criar descrição para a movimentação
                var descricao = $"Pedido {pedido.NumeroPedido} - {pedido.TipoPedido}";
                if (pedido.Cliente != null)
                {
                    descricao += $" - {pedido.Cliente.NomeCompleto}";
                }

                // Montar observação da movimentação, incluindo troco quando aplicável
                var observacaoMov = observacoes ?? $"Fechamento pedido {pedido.NumeroPedido}";

                if (formaPagamento == FormaPagamento.Dinheiro && valorRecebido.HasValue && valorRecebido > pedido.Total)
                {
                    var troco = valorRecebido.Value - pedido.Total;
                    observacaoMov += $" | Troco: R$ {troco:N2} (Recebido: R$ {valorRecebido.Value:N2} • Total: R$ {pedido.Total:N2})";
                }

                // Registrar movimentação de entrada no caixa
                var movimentacaoViewModel = new MovimentacaoViewModel
                {
                    Tipo = TipoMovimentacao.Entrada,
                    Valor = pedido.Total,
                    FormaPagamento = formaPagamento,
                    Categoria = CategoriaFinanceira.Venda,
                    Descricao = descricao,
                    Observacoes = observacaoMov,
                    IsSangria = false,
                    IsSuprimento = false
                };

                await financeiroService.AdicionarMovimentacao(movimentacaoViewModel);

                await transaction.CommitAsync();
                return (true, "Pedido fechado e pagamento registrado com sucesso!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Erro ao processar fechamento: {ex.Message}");
            }
        }
    }
}