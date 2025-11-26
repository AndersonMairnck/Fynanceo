// Models/Estoque/Estoque.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fynanceo.Models.Enums;

namespace Fynanceo.Models
{
    public class Estoque
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter até 100 caracteres")]
        public string Nome { get; set; }

        [StringLength(20)]
        public string Codigo { get; set; }

        [StringLength(500)]
        public string Descricao { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O estoque atual não pode ser negativo")]
        public decimal EstoqueAtual { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O estoque mínimo não pode ser negativo")]
        public decimal EstoqueMinimo { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O estoque máximo não pode ser negativo")]
        public decimal EstoqueMaximo { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O custo unitário não pode ser negativo")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CustoUnitario { get; set; }

        [Required]
        public UnidadeMedida UnidadeMedida { get; set; }

        
        public StatusEstoque Status { get; set; } = StatusEstoque.Ativo;

     
        public string Categorias { get; set; }

        public int? FornecedorId { get; set; }
        public Fornecedor Fornecedor { get; set; }

        // Auditoria
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        // Navegação
        public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; }
        public ICollection<ProdutoIngrediente> ProdutoIngredientes { get; set; }
    }

    // Models/Estoque/MovimentacaoEstoque.cs
    public class MovimentacaoEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EstoqueId { get; set; }
        public Estoque Estoque { get; set; }

        [Required]
        public TipoMovimentacaoEstoque Tipo { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O custo unitário não pode ser negativo")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CustoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CustoTotal { get; set; }

        [StringLength(50)]
        public string Documento { get; set; } // NF, Pedido, etc.

        [StringLength(500)]
        public string Observacao { get; set; }

        // Relacionamentos
        public int? FornecedorId { get; set; }
        public Fornecedor Fornecedor { get; set; }

        public int? PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        // Auditoria
        public DateTime DataMovimentacao { get; set; } = DateTime.Now;
        public string Usuario { get; set; }
    }

    // Models/Estoque/CategoriaEstoque.cs
    public class CategoriaEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(50, ErrorMessage = "O nome deve ter até 50 caracteres")]
        public string Nome { get; set; }

        [StringLength(200)]
        public string Descricao { get; set; }


        public StatusEstoque Status { get; set; } = StatusEstoque.Ativo;

        // Navegação
        public ICollection<Estoque> Estoques { get; set; }
    }

    // Models/Estoque/Inventario.cs
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Descricao { get; set; }

       
        public StatusInventario Status { get; set; } = StatusInventario.Aberto;

        public DateTime DataAbertura { get; set; } = DateTime.Now;
        public DateTime? DataFechamento { get; set; }

        [StringLength(500)]
        public string Observacao { get; set; }

        // Navegação
        public ICollection<ItemInventario> Itens { get; set; }
    }

    // Models/Estoque/ItemInventario.cs
    public class ItemInventario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventarioId { get; set; }
        public Inventario Inventario { get; set; }

        [Required]
        public int EstoqueId { get; set; }
        public Estoque Estoque { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal QuantidadeSistema { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal QuantidadeFisica { get; set; }

        public decimal Diferenca => QuantidadeFisica - QuantidadeSistema;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CustoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CustoTotalDiferenca => Diferenca * CustoUnitario;

        [StringLength(200)]
        public string Observacao { get; set; }

        public bool Conferido { get; set; } = false;
    }

    // Models/Estoque/ProdutoIngrediente.cs (Integração com módulo de Produtos)
    public class ProdutoIngrediente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }

        [Required]
        public int EstoqueId { get; set; }
        public Estoque Estoque { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        [StringLength(50)]
        public string UnidadeMedida { get; set; }

        [StringLength(200)]
        public string Observacao { get; set; }
    }

   
}