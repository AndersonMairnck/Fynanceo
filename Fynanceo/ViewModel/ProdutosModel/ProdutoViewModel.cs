using System.ComponentModel.DataAnnotations;
using Fynanceo.Models.Enums;
using Fynanceo.Models;

namespace Fynanceo.ViewModel.ProdutosModel
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
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; }

        [Required(ErrorMessage = "Subcategoria é obrigatória")]
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
        public decimal PercentualLucro => CustoUnitario > 0 ? (ValorVenda - CustoUnitario) / CustoUnitario * 100 : 0;

        [Required(ErrorMessage = "Tempo de preparo é obrigatório")]
        [Range(0, 480, ErrorMessage = "Tempo deve ser entre 1 e 480 minutos")]
        [Display(Name = "Tempo de Preparo (minutos)")]
        public int TempoPreparoMinutos { get; set; }

        [Display(Name = "Tempo Extra para Pico (minutos)")]
        public int TempoExtraPico { get; set; } = 0;

        [Required(ErrorMessage = "Senao tiver Personalizações coloque informações que não possui")]
        [Display(Name = "Opções de Personalização")]
        public string OpcoesPersonalizacao { get; set; }

        [Display(Name = "Disponível")]
        public bool Disponivel { get; set; } = true;

        [Display(Name = "Motivo da Indisponibilidade")]
        public string? MotivoIndisponibilidade { get; set; }

        

        public List<string>? Categorias { get; set; }

        // Lista de ingredientes para a view
        public List<MateriaisProdutoViewModel>? Ingredientes { get; set; } = new List<MateriaisProdutoViewModel>();
    }

    public class MateriaisProdutoViewModel
    {
        public int IdEstoque { get; set; }

       // [Required(ErrorMessage = "Nome do ingrediente é obrigatório")]
        [Display(Name = "Nome do Ingrediente")]
        public string Nome { get; set; }

     //   [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        [Display(Name = "Quantidade")]
        public decimal Quantidade { get; set; }

      //  [Required(ErrorMessage = "Unidade de medida é obrigatória")]
        [Display(Name = "Unidade de Medida")]
        public StatusUnidadeMedida UnidadeMedida { get; set; }
        
        [StringLength(20)]
        public string Codigo { get; set; }
        
        [Key]
        public int Id { get; set; }
        
       // public Produto Produto { get; set; }

    

        


        [StringLength(200)]
        public string? Observacao { get; set; }

    
    }
}