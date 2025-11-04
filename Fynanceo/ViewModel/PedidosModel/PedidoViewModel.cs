// ViewModels/PedidoViewModel.cs
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.PedidosModel
{
    public class PedidoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número do Pedido")]
        public string? NumeroPedido { get; set; }

        [Required(ErrorMessage = "Tipo do pedido é obrigatório")]
        [Display(Name = "Tipo do Pedido")]
        public TipoPedido TipoPedido { get; set; }

        [Display(Name = "Mesa")]
        public int? MesaId { get; set; }

        [Display(Name = "Cliente")]
        public int? ClienteId { get; set; }

        [Display(Name = "Endereço de Entrega")]
        public int? EnderecoEntregaId { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Taxa de Entrega")]
        public decimal TaxaEntrega { get; set; }

        // Para seleção
        public List<Mesa>? MesasDisponiveis { get; set; }
        public List<Cliente>? Clientes { get; set; }
        public List<EnderecoCliente>? EnderecosEntrega { get; set; }
        public List<Produto>? ProdutosDisponiveis { get; set; }
        public List<string>? CategoriasDisponiveis { get; set; }

        // Itens do pedido
        public List<ItemPedidoViewModel> Itens { get; set; } = new List<ItemPedidoViewModel>();
    }

    public class ItemPedidoViewModel
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public decimal PrecoUnitario { get; set; }

        [Range(0.01, 100, ErrorMessage = "Quantidade deve ser entre 0.01 e 100")]
        public int Quantidade { get; set; }
        public string? Observacoes { get; set; }
        public string? Personalizacoes { get; set; }
    }

    public class CozinhaViewModel
    {
        public List<Pedido> PedidosCozinha { get; set; } = new();
        public List<Pedido> PedidosPreparo { get; set; } = new();
        public List<Pedido> PedidosProntos { get; set; } = new();

        public CozinhaConfig Config { get; set; } = new CozinhaConfig();
    }
   
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(string message, T data) => new() { Success = true, Message = message, Data = data };
        public static ApiResponse<T> Fail(string message) => new() { Success = false, Message = message };
    }
}