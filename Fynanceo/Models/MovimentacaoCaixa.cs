// Models/MovimentacaoCaixa.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fynanceo.Models.Enums;

namespace Fynanceo.Models
{
    public class MovimentacaoCaixa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CaixaId { get; set; }
        public Caixa Caixa { get; set; }

        [Required]
        public TipoMovimentacao Tipo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [Required]
        public FormaPagamento FormaPagamento { get; set; }

        [Required]
        public CategoriaFinanceira Categoria { get; set; }

        [Required]
        [StringLength(200)]
        public string Descricao { get; set; }

        public string? Observacoes { get; set; }

        // Relacionamentos opcionais
        public int? PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        public int? EntregaId { get; set; }
        public Entrega? Entrega { get; set; }

        public int? FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        [Required]
        public DateTime DataMovimentacao { get; set; } = DateTime.Now;

        [Required]
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }

        // Para sangrias e suprimentos
        public bool? IsSangria { get; set; } = false;
        public bool? IsSuprimento { get; set; } = false;
    }
}