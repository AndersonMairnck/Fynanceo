// ViewModels/FechamentoPedidoViewModel.cs
using Fynanceo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.PedidosModel
{
    public class FechamentoPedidoViewModel
    {
        public int PedidoId { get; set; }
        
        [Display(Name = "Número do Pedido")]
        public string NumeroPedido { get; set; }
        
        [Display(Name = "Total do Pedido")]
        public decimal TotalPedido { get; set; }
        
        [Required(ErrorMessage = "Forma de pagamento é obrigatória")]
        [Display(Name = "Forma de Pagamento")]
        public FormaPagamento FormaPagamento { get; set; }
        
        [Display(Name = "Valor Recebido")]
        [Range(0.01, 999999, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal? ValorRecebido { get; set; }
        
        [Display(Name = "Troco")]
        public decimal Troco => ValorRecebido.HasValue ? 
            (ValorRecebido.Value - TotalPedido) : 0;
        
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }
        
        // Para mostrar informações do pedido
        public string? ClienteNome { get; set; }
        public string? MesaNumero { get; set; }
        public TipoPedido TipoPedido { get; set; }
    }
}