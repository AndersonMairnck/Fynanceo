
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using Fynanceo.Service.Interface;

using Fynanceo.ViewModel.PedidosModel;
using Microsoft.AspNetCore.Mvc;




namespace Fynanceo.Controllers
{
    public class PedidosController : Controller
    {
      
        private readonly IPedidoService _pedidoService;
     
        private readonly IProdutoService _produtoService;
        private readonly IClienteService _clienteService;
        private readonly IConfigService _configService;
        private readonly IEntregaService _entregaService;


        public PedidosController(IPedidoService pedidoService,  
                                    IProdutoService produtoService, 
                                        IClienteService clienteService, 
                                            IConfigService configService,
                                                IEntregaService entregaService)
        {

            _pedidoService = pedidoService;
            //_mesaService = mesaService;
            _produtoService = produtoService;
            _clienteService = clienteService;
            _configService = configService;
            _entregaService = entregaService;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var pedidos = await _pedidoService.ObterPedidosDoDia();
            return View(pedidos);
        }

       
        public async Task<IActionResult> Create()
        {
            var viewModel = new PedidoViewModel
            {
               // MesasDisponiveis = await _mesaService.ObterPorStatusAsync("Livre"),
                Clientes = await _clienteService.ObterTodosAsync(),
                ProdutosDisponiveis = await _produtoService.ObterProdutosPopularesAsync(10),
                CategoriasDisponiveis = await _produtoService.ObterCategoriasAsync()
            };

            return View(viewModel);
        }


        // POST: Pedidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PedidoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var pedido = await _pedidoService.CriarPedido(viewModel);



                    TempData["Success"] = "Pedido criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = pedido.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao criar pedido: {ex.Message}");
                }
            }

            //Recarregar dados se houver erro
           // viewModel.MesasDisponiveis = await _mesaService.ObterPorStatusAsync("Livre");
            viewModel.Clientes = await _clienteService.ObterTodosAsync();

            viewModel.ProdutosDisponiveis = await _produtoService.ObterTodosAsync();

            return View(viewModel);
        }

        // GET: Pedidos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var pedido = await _pedidoService.ObterPedidoCompleto(id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // GET: Pedidos/Cozinha
        public async Task<IActionResult> Cozinha()
        {
            var config = await _configService.ObterConfigCozinhaAsync();

            var viewModel = new CozinhaViewModel
            {
                PedidosCozinha = await _pedidoService.ObterPedidosPorStatus("EnviadoCozinha"),
                PedidosPreparo = await _pedidoService.ObterPedidosPorStatus("EmPreparo"),
                PedidosProntos = await _pedidoService.ObterPedidosPorStatus("Pronto"),
                Config = config // Agora vem do banco
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> ObterEstadoCozinha()
        {
            var pedidosCozinha = await _pedidoService.ObterPedidosPorStatus("EnviadoCozinha");
            var pedidosPreparo = await _pedidoService.ObterPedidosPorStatus("EmPreparo");
            var pedidosProntos = await _pedidoService.ObterPedidosPorStatus("Pronto");

            return Json(new
            {
                pedidosCozinhaCount = pedidosCozinha.Count,
                pedidosPreparoCount = pedidosPreparo.Count,
                pedidosProntosCount = pedidosProntos.Count
            });
        }


        // POST: Pedidos/AtualizarStatus/5
        [HttpPost]
        public async Task<IActionResult> AtualizarStatus(int id, string novoStatus)
        {
            try
            {
                var usuario = "Usuário"; // Temporário - depois pegar do usuário logado
                var pedido = await _pedidoService.AtualizarStatus(id, novoStatus, usuario);


                return Json(new { success = true, message = "Status atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        
        public async Task<JsonResult> GetEnderecosCliente(int clienteId)
        {
            var cliente = await _clienteService.ObterPorIdAsync(clienteId);
            if (cliente == null || cliente.Enderecos == null)
                return Json(new List<object>());

            var enderecos = cliente.Enderecos.Select(e => new
            {
                id = e.Id,
                enderecoCompleto = $"{e.Logradouro}, {e.Numero} - {e.Bairro} - {e.Cidade}/{e.Estado}"
            });

            return Json(enderecos);
        }

        // POST: Pedidos/AdicionarItem
        [HttpPost]
        public async Task<JsonResult> AdicionarItem([FromBody] ItemPedidoViewModel itemVm)
        {
            try
            {
                var pedido = await _pedidoService.AdicionarItem(itemVm.ProdutoId, itemVm);
                return Json(new { success = true, message = "Item adicionado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // No PedidosController.cs
        [HttpPost]

        public async Task<IActionResult> IniciarPreparoItem(int itemId)
        {
            if (itemId <= 0)
                return BadRequest(new { success = false, message = "ID do item inválido." });

            try
            {
                var item = await _pedidoService.IniciarPreparoItemAsync(itemId);
                if (item == null)
                    return NotFound(new { success = false, message = "Item não encontrado." });

                // return Ok(new { success = true, message = "Preparo iniciado com sucesso.", data = item });
                return Ok(ApiResponse<ItemPedido>.Ok("Preparo iniciado", item));

            }
            catch (Exception ex)
            {
                // Logar exceção internamente
                // _logger.LogError(ex, "Erro ao iniciar preparo do item {ItemId}", itemId);
                return StatusCode(500, new { success = false, message = "Erro interno ao iniciar preparo do item." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> MarcarProntoItem(int itemId)
        {



            try
            {
                var item = await _pedidoService.MarcarProntoItemAsync(itemId);
                if (item == null)
                    return Json(new { success = false, message = "Item não encontrado" });



                return Json(new { success = true, message = "Item marcado como pronto", item });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> IniciarPreparoTodos(int pedidoId)
        {
            try
            {
                var sucesso = await _pedidoService.IniciarPreparoTodosAsync(pedidoId);


                return Json(new { success = true, message = "Preparo iniciado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao iniciar preparo: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> MarcarProntoTodos(int pedidoId)
        {
            try
            {
                var sucesso = await _pedidoService.MarcarProntoTodosAsync(pedidoId);
                if (sucesso == null)
                    return Json(new { success = false, message = "Nenhum item pendente para marcar como pronto" });

                return Json(new { success = true, message = "Todos os itens marcados como prontos" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]

        public async Task<IActionResult> EntregaIndividualCozinha(int itemId, int pedidoId)
        {
            if (itemId <= 0)
                return BadRequest(new { success = false, message = "ID do item inválido." });

            try
            {
                var item = await _pedidoService.EntregueIndividualCozinha(itemId);
                if (item == null)
                    return NotFound(new { success = false, message = "Item não encontrado." });

                // return Ok(new { success = true, message = "Preparo iniciado com sucesso.", data = item });
                return Ok(ApiResponse<ItemPedido>.Ok("Preparo iniciado", item));

            }
            catch (Exception ex)
            {
                // Logar exceção internamente
                // _logger.LogError(ex, "Erro ao iniciar preparo do item {ItemId}", itemId);
                return StatusCode(500, new { success = false, message = "Erro interno ao iniciar preparo do item." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> EntregaTodosCozinha(int pedidoId)
        {
            try
            {
                var sucesso = await _pedidoService.EntregaTodosCozinha(pedidoId);


                return Json(new { success = true, message = "entrega iniciado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao iniciar preparo: " + ex.Message });
            }
        }
        public async Task<IActionResult> GetProdutosPaginados(int page = 1, int pageSize = 20, string search = "", string categoria = "")
        {
            try
            {
                var resultTuple = await _produtoService.ObterProdutosPaginadosAsync(page, pageSize, search, categoria);
                var produtos = resultTuple.Produtos;
                var totalCount = resultTuple.TotalCount;

                var result = produtos.Select(p => new
                {
                    p.Id,
                    p.Nome,
                    p.Descricao,
                    p.Categoria,
                    Subcategoria = p.Subcategoria ?? "",
                    p.ValorVenda
                }).ToList();

                return Json(new
                {
                    produtos = result,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        public async Task<IActionResult> ConfigCozinha()
        {
            var config = await _configService.ObterConfigCozinhaAsync();
            return View(config);
        }

        // POST: Configurações da Cozinha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigCozinha(CozinhaConfig config)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _configService.AtualizarConfigCozinhaAsync(config);
                    TempData["Success"] = "Configurações atualizadas com sucesso!";
                    return RedirectToAction(nameof(Cozinha));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao salvar configurações: {ex.Message}");
                }
            }

            return View(config);
        }
        [HttpPost]
        public async Task<IActionResult> EnviaParaEntrega(int Id,string novoStatus)
        {
            try
            {
              
                await  _entregaService.CriarEntrega(Id);
                await AtualizarStatus(Id, novoStatus);


                return Json(new { success = true, message = "Status atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        // NO PedidosController.cs - ADICIONAR ESTES MÉTODOS

// GET: Pedidos/FecharComPagamento/5
public async Task<IActionResult> FecharComPagamento(int id)
{
    var pedido = await _pedidoService.ObterPedidoCompleto(id);
    if (pedido == null)
    {
        return NotFound();
    }

    // Verificar se o pedido já está fechado
    if (pedido.Status == PedidoStatus.Fechado)
    {
        TempData["Warning"] = "Pedido já está fechado!";
        return RedirectToAction(nameof(Details), new { id });
    }

    var viewModel = new FechamentoPedidoViewModel
    {
        PedidoId = pedido.Id,
        NumeroPedido = pedido.NumeroPedido,
        TotalPedido = pedido.Total,
        ClienteNome = pedido.Cliente?.NomeCompleto,
        MesaNumero = pedido.Mesa?.Numero,
        TipoPedido = pedido.TipoPedido
        
    };

    return View(viewModel);
}

// POST: Pedidos/FecharComPagamento
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> FecharComPagamento(FechamentoPedidoViewModel viewModel)
{
    if (ModelState.IsValid)
    {
        try
        {
            // Verificar se valor recebido é suficiente para formas que precisam de troco
            if (viewModel.FormaPagamento == FormaPagamento.Dinheiro && 
                viewModel.ValorRecebido.HasValue && 
                viewModel.ValorRecebido.Value < viewModel.TotalPedido)
            {
                ModelState.AddModelError("ValorRecebido", "Valor recebido é menor que o total do pedido");
                return View(viewModel);
            }

            // Fechar pedido e registrar pagamento
            var resultado = await _pedidoService.FecharPedidoComPagamentoAsync(
                viewModel.PedidoId, 
                viewModel.FormaPagamento, 
                viewModel.ValorRecebido,
                viewModel.Observacoes);

            if (resultado.Success)
            {
                TempData["Success"] = "Pedido fechado e pagamento registrado com sucesso!";
                
                // Se houver troco, mostrar alerta
                if (viewModel.Troco > 0)
                {
                    TempData["Info"] = $"Troco: R$ {viewModel.Troco:N2}";
                }
                
                return RedirectToAction(nameof(Details), new { id = viewModel.PedidoId });
            }
            else
            {
                ModelState.AddModelError("", resultado.Message);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Erro ao fechar pedido: {ex.Message}");
        }
    }

    // Recarregar dados se houver erro
    var pedido = await _pedidoService.ObterPedidoCompleto(viewModel.PedidoId);
    if (pedido != null)
    {
        viewModel.ClienteNome = pedido.Cliente?.NomeCompleto;
        viewModel.MesaNumero = pedido.Mesa?.Numero;
        viewModel.TipoPedido = pedido.TipoPedido;
    }

    return View(viewModel);
}
    }
}