using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Código é obrigatório")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Required]
        public string Categoria { get; set; }

        public string Subcategoria { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Custo deve ser maior que zero")]
        public decimal CustoUnitario { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor de venda deve ser maior que zero")]
        public decimal ValorVenda { get; set; }

        public decimal MargemLucro => ValorVenda - CustoUnitario;

        [Range(0, 480)]
        public int TempoPreparoMinutos { get; set; }

        public int TempoExtraPico { get; set; } = 0;

        public string OpcoesPersonalizacao { get; set; } 
        
        public bool Disponivel { get; set; } = true;


        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Navegação
        public virtual ICollection<ProdutoIngrediente>? Ingredientes { get; set; } = new List<ProdutoIngrediente>();
    }

    // public class IngredienteProduto
    // {
    //     [Key]
    //     public int Id { get; set; }
    //
    //    
    //     public string Nome { get; set; }
    //
    //     [Range(0.001, double.MaxValue)]
    //     public decimal Quantidade { get; set; }
    //
    //
    //     public string? UnidadeMedida { get; set; } // g, kg, ml, L, un
    //
    //     public int ProdutoId { get; set; }
    //     public virtual Produto Produto { get; set; }
    //
    //  
    // }
}