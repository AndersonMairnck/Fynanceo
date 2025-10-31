// ViewModels/EntregaViewModel.cs
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModels
{
    public class EntregaViewModel
    {
        public int Id { get; set; }
        public string? NumeroPedido { get; set; }

        [Required]
        [Display(Name = "Pedido")]
        public int PedidoId { get; set; }

        [Display(Name = "Entregador")]
        public int? EntregadorId { get; set; }

        [Display(Name = "Status")]
        public StatusEntrega Status { get; set; }

        // Informações do cliente
        public string? ClienteNome { get; set; }
        public string? ClienteTelefone { get; set; }

        [Display(Name = "Endereço Completo")]
        public string EnderecoCompleto { get; set; }

        [Display(Name = "Complemento")]
        public string? Complemento { get; set; }

        [Display(Name = "Referência")]
        public string? Referencia { get; set; }

        [Display(Name = "Instruções Especiais")]
        public string? Instrucoes { get; set; }

        [Display(Name = "Taxa de Entrega")]
        public decimal TaxaEntrega { get; set; }

        [Display(Name = "Comissão do Entregador")]
        public decimal ComissaoEntregador { get; set; }

        [Display(Name = "Código de Verificação")]
        public string? CodigoVerificacao { get; set; }

        // Para seleção
        public List<Entregador>? EntregadoresDisponiveis { get; set; }
        public List<Pedido>? PedidosPendentes { get; set; }

        // Mapa
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}