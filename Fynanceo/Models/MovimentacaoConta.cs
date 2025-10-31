// Models/MovimentacaoConta.cs
using Fynanceo.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fynanceo.Models
{
    public class MovimentacaoConta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContaId { get; set; }
        public Conta Conta { get; set; }

        [Required]
        public StatusConta StatusAnterior { get; set; }

        [Required]
        public StatusConta StatusNovo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorPago { get; set; }

        public DateTime? DataPagamento { get; set; }

        public string? Observacao { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }

        public DateTime DataAlteracao { get; set; } = DateTime.Now;
    }
}