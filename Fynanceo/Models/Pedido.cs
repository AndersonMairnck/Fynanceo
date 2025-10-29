// Models/Pedido.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fynanceo.Models.Enums;

namespace Fynanceo.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Número do pedido é obrigatório")]
        public string NumeroPedido { get; set; } 

        [Required(ErrorMessage = "Tipo do pedido é obrigatório")]
        public TipoPedido TipoPedido { get; set; }

        [Required(ErrorMessage = "Status é obrigatório")]
        public PedidoStatus Status { get; set; } = PedidoStatus.Aberto;

        public PrioridadePedido Prioridade { get; set; } = PrioridadePedido.Normal;

        // Relacionamentos
        public int? MesaId { get; set; }
        public Mesa? Mesa { get; set; }

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int? FuncionarioId { get; set; }
        public Funcionario? Funcionario { get; set; }

        // Delivery
        public int? EnderecoEntregaId { get; set; }
        public EnderecoCliente? EnderecoEntrega { get; set; }

        public decimal TaxaEntrega { get; set; } = 0;
        public string? Observacoes { get; set; }

        // Tempos
        public DateTime DataAbertura { get; set; } = DateTime.Now;
        public DateTime? DataEnvioCozinha { get; set; }
        public DateTime? DataPreparo { get; set; }
        public DateTime? DataPronto { get; set; }
        public DateTime? DataEntrega { get; set; }
        public DateTime? DataFechamento { get; set; }

        // Financeiro
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Navigation Properties
        public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
        public ICollection<HistoricoPedido> Historico { get; set; } = new List<HistoricoPedido>();
    }
}