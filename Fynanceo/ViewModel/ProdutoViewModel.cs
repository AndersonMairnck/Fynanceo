using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModels
{
    public class ProdutoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Código é obrigatório")]
        [Display(Name = "Código do Produto")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
        [Display(Name = "Nome do Produto")]
        public string Nome { get; set; }

        [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; }

        [Display(Name = "Subcategoria")]
        public string Subcategoria { get; set; }

        [Required(ErrorMessage = "Custo unitário é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Custo deve ser maior que zero")]
        [Display(Name = "Custo Unitário")]
        public decimal CustoUnitario { get; set; }

        [Required(ErrorMessage = "Valor de venda é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor de venda deve ser maior que zero")]
        [Display(Name = "Valor de Venda")]
        public decimal ValorVenda { get; set; }

        [Display(Name = "Margem de Lucro")]
        public decimal MargemLucro => ValorVenda - CustoUnitario;

        [Display(Name = "Percentual de Lucro")]
        public decimal PercentualLucro => CustoUnitario > 0 ? ((ValorVenda - CustoUnitario) / CustoUnitario) * 100 : 0;

        [Required(ErrorMessage = "Tempo de preparo é obrigatório")]
        [Range(1, 480, ErrorMessage = "Tempo deve ser entre 1 e 480 minutos")]
        [Display(Name = "Tempo de Preparo (minutos)")]
        public int TempoPreparoMinutos { get; set; }

        [Display(Name = "Tempo Extra para Pico (minutos)")]
        public int TempoExtraPico { get; set; } = 0;

        [Display(Name = "Opções de Personalização")]
        public string OpcoesPersonalizacao { get; set; }

        [Display(Name = "Disponível")]
        public bool Disponivel { get; set; } = true;

        [Display(Name = "Motivo da Indisponibilidade")]
        public string? MotivoIndisponibilidade { get; set; }

        



        // Lista de ingredientes para a view
        public List<IngredienteViewModel> Ingredientes { get; set; } = new List<IngredienteViewModel>();
    }

    public class IngredienteViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome do ingrediente é obrigatório")]
        [Display(Name = "Nome do Ingrediente")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        [Display(Name = "Quantidade")]
        public decimal Quantidade { get; set; }

        [Required(ErrorMessage = "Unidade de medida é obrigatória")]
        [Display(Name = "Unidade de Medida")]
        public string UnidadeMedida { get; set; }
    }
}