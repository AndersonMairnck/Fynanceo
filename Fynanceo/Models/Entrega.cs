// Models/Entrega.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fynanceo.Models.Enums;

namespace Fynanceo.Models
{
    public class Entrega
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public int? EntregadorId { get; set; }
        public Entregador? Entregador { get; set; }

        [Required]
        public StatusEntrega Status { get; set; } = StatusEntrega.AguardandoEntregador;

        // Informações de entrega
        [Required]
        public string EnderecoCompleto { get; set; }

        public string? Complemento { get; set; }
        public string? Referencia { get; set; }
        public string? Instrucoes { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Tempos
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataSaiuEntrega { get; set; }
        public DateTime? DataPrevisao { get; set; }
        public DateTime? DataEntrega { get; set; }
        public DateTime? DataCancelamento { get; set; }

        // Valores
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxaEntrega { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ComissaoEntregador { get; set; }

        // Controle
        public string? CodigoVerificacao { get; set; }
        public string? MotivoProblema { get; set; }
        public string? Observacoes { get; set; }

        // Avaliação
        public int? Avaliacao { get; set; }
        public string? ComentarioAvaliacao { get; set; }

        // Navigation Properties
        public ICollection<HistoricoEntrega> Historico { get; set; } = new List<HistoricoEntrega>();
    }
}