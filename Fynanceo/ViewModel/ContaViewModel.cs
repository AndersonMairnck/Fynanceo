// ViewModels/ContaViewModel.cs
using Fynanceo.Models;
using Fynanceo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModels
{
    public class ContaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Descrição é obrigatória")]
        [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Tipo é obrigatório")]
        [Display(Name = "Tipo")]
        public TipoMovimentacao Tipo { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public CategoriaFinanceira Categoria { get; set; }

        [Required(ErrorMessage = "Valor é obrigatório")]
        [Range(0.01, 999999, ErrorMessage = "Valor deve ser maior que zero")]
        [Display(Name = "Valor")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Data de vencimento é obrigatória")]
        [Display(Name = "Data de Vencimento")]
        [DataType(DataType.Date)]
        public DateTime DataVencimento { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Forma de Pagamento")]
        public FormaPagamento? FormaPagamento { get; set; }

        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }

        [Display(Name = "Parcela Atual")]
        public int? ParcelaAtual { get; set; }

        [Display(Name = "Total de Parcelas")]
        public int? TotalParcelas { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        // Para seleção
        public List<Fornecedor>? Fornecedores { get; set; }
    }

    public class PagamentoContaViewModel
    {
        public int ContaId { get; set; }

        [Required(ErrorMessage = "Valor pago é obrigatório")]
        [Range(0.01, 999999, ErrorMessage = "Valor deve ser maior que zero")]
        [Display(Name = "Valor Pago")]
        public decimal ValorPago { get; set; }

        [Required(ErrorMessage = "Data de pagamento é obrigatória")]
        [Display(Name = "Data de Pagamento")]
        [DataType(DataType.Date)]
        public DateTime DataPagamento { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Forma de pagamento é obrigatória")]
        [Display(Name = "Forma de Pagamento")]
        public FormaPagamento FormaPagamento { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }
    }
}