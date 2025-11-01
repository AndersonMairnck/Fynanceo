// ViewModels/MovimentacaoViewModel.cs
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.FinanceirosModel
{
    public class MovimentacaoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tipo é obrigatório")]
        [Display(Name = "Tipo")]
        public TipoMovimentacao Tipo { get; set; }

        [Required(ErrorMessage = "Valor é obrigatório")]
        [Range(0.01, 999999, ErrorMessage = "Valor deve ser maior que zero")]
        [Display(Name = "Valor")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Forma de pagamento é obrigatória")]
        [Display(Name = "Forma de Pagamento")]
        public FormaPagamento FormaPagamento { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public CategoriaFinanceira Categoria { get; set; }

        [Required(ErrorMessage = "Descrição é obrigatória")]
        [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "É Sangria?")]
        public bool IsSangria { get; set; }

        [Display(Name = "É Suprimento?")]
        public bool IsSuprimento { get; set; }

        // Para seleção
        public List<Caixa>? CaixasAbertos { get; set; }
    }
}