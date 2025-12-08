// Models/Conta.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fynanceo.Models.Enums;

namespace Fynanceo.Models
{
    public class Conta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Descricao { get; set; }

        [Required]
        public TipoMovimentacao Tipo { get; set; }

        [Required]
        public CategoriaFinanceira Categoria { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorPago { get; set; } = 0;

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataPagamento { get; set; }

        [Required]
        public StatusConta Status { get; set; } = StatusConta.Pendente;

        public FormaPagamento? FormaPagamento { get; set; }

        // Para contas parceladas
        public int? ParcelaAtual { get; set; }
        public int? TotalParcelas { get; set; }

        // Relacionamentos
        public int? FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        public int? PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        public string? Observacoes { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataAlteracao { get; set; }

        // Navigation Properties
        public ICollection<MovimentacaoConta> Movimentacoes { get; set; } = new List<MovimentacaoConta>();
    }
}