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
        public string Descricao { get; set; }

        [Required]
        public string Categoria { get; set; }

        public string Subcategoria { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Custo deve ser maior que zero")]
        public decimal CustoUnitario { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Valor de venda deve ser maior que zero")]
        public decimal ValorVenda { get; set; }

        public decimal MargemLucro => ValorVenda - CustoUnitario;

        [Range(1, 480)]
        public int TempoPreparoMinutos { get; set; }

        public int TempoExtraPico { get; set; } = 0;

        public string OpcoesPersonalizacao { get; set; } // JSON com opções de tamanho, sabores, etc.
        
        public bool Disponivel { get; set; } = true;
       // public string MotivoIndisponibilidade { get; set; }

        //// Tributação
        //public string CodigoNCM { get; set; }
        //public string Origem { get; set; }
        //public string CST { get; set; }
        //public decimal Aliquota { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Navegação
        public virtual ICollection<IngredienteProduto> Ingredientes { get; set; } = new List<IngredienteProduto>();
    }

    public class IngredienteProduto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Range(0.001, double.MaxValue)]
        public decimal Quantidade { get; set; }

        [Required]
        public string UnidadeMedida { get; set; } // g, kg, ml, L, un

        public int ProdutoId { get; set; }
        public virtual Produto Produto { get; set; }
    }
}