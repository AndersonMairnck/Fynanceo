// ViewModels/CaixaViewModel.cs
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.CaixaModel
{
    public class CaixaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Saldo inicial é obrigatório")]
        [Range(0, 99999, ErrorMessage = "Saldo inicial deve ser positivo")]
        [Display(Name = "Saldo Inicial")]
        public decimal SaldoInicial { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        // Para fechamento
        [Display(Name = "Saldo Físico")]
        public decimal? SaldoFisico { get; set; }

        public List<MovimentacaoCaixa>? Movimentacoes { get; set; }
    }

    public class FechamentoCaixaViewModel
    {
        public int CaixaId { get; set; }
        public decimal SaldoFinal { get; set; }

        [Required(ErrorMessage = "Saldo físico é obrigatório")]
        [Display(Name = "Saldo Físico Conferido")]
        public decimal SaldoFisico { get; set; }

        [Display(Name = "Observações do Fechamento")]
        public string? Observacoes { get; set; }
    }
}