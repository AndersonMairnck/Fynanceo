// Models/Caixa.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fynanceo.Models.Enums;

namespace Fynanceo.Models
{
    public class Caixa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

        public DateTime? DataFechamento { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoInicial { get; set; }

        // [Column(TypeName = "decimal(18,2)")]
        // public decimal TotalEntradas { get; set; }
        //
        // [Column(TypeName = "decimal(18,2)")]
        // public decimal TotalSaidas { get; set; }
        [NotMapped] // Esta propriedade não será mapeada para o banco
        public decimal TotalSaidas => Movimentacoes?
            .Where(m => m.Tipo == TipoMovimentacao.Saida)
            .Sum(m => m.Valor) ?? 0;
        [NotMapped] // Esta propriedade não será mapeada para o banco
        public decimal TotalEntradas => Movimentacoes?
            .Where(m => m.Tipo == TipoMovimentacao.Entrada)
            .Sum(m => m.Valor) ?? 0;
    
        [Column(TypeName = "decimal(18,2)")]
        [NotMapped]
        public decimal SaldoFinal => SaldoInicial + TotalEntradas - TotalSaidas;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoFisico { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [NotMapped]
        public decimal Diferenca =>
            SaldoFisico > SaldoFinal
                ? SaldoFisico - SaldoFinal
                : SaldoFinal - SaldoFisico;

        [Required]
        public int UsuarioAberturaId { get; set; }
        public string UsuarioAberturaNome { get; set; }

        public int? UsuarioFechamentoId { get; set; }
        public string? UsuarioFechamentoNome { get; set; }

        public string? Observacoes { get; set; }
        public bool Fechado { get; set; } = false;

        // Navigation Properties
        public ICollection<MovimentacaoCaixa> Movimentacoes { get; set; } = new List<MovimentacaoCaixa>();
    }
}