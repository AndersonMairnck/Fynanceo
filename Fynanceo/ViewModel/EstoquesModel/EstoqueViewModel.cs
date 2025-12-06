using Fynanceo.Models;
using System.ComponentModel.DataAnnotations;
using Fynanceo.Models.Enums;

namespace Fynanceo.ViewModel.EstoquesModel
{
    public class EstoqueViewModel
    {
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
        public decimal CustoUnitario { get; set; }

        [Required]
        public StatusUnidadeMedida StatusUnidadeMedida { get; set; }

        [Required]
        public StatusEstoque Status { get; set; }

        [Display(Name = "Categoria")]
        public int? CategoriaEstoqueId { get; set; }

        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }
        public string Categoria { get; set; }
        // Para dropdowns
        public List<string> Categorias { get; set; }
        public List<Fornecedor> Fornecedores { get; set; }
    }

    // ViewModels/Estoque/MovimentacaoEstoqueViewModel.cs
    public class MovimentacaoEstoqueViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione o produto")]
        [Display(Name = "Produto")]
        public int EstoqueId { get; set; }

        [Required]
        public TipoMovimentacaoEstoque Tipo { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O custo unitário não pode ser negativo")]
        public decimal CustoUnitario { get; set; }

        [Display(Name = "Documento")]
        [StringLength(50)]
        public string Documento { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500)]
        public string Observacao { get; set; }

        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }

        [Display(Name = "Pedido")]
        public int? PedidoId { get; set; }

        public string? UsuarioNome { get; set; }

        // Para dropdowns
        public List<Estoque>? Produtos { get; set; }
        public List<Fornecedor>? Fornecedores { get; set; }
        public List<Pedido>? Pedidos { get; set; }
    }

    // ViewModels/Estoque/DashboardEstoqueViewModel.cs
    public class DashboardEstoqueViewModel
    {
        public int TotalProdutos { get; set; }
        public int ProdutosEstoqueMinimo { get; set; }
        public int ProdutosEstoqueZero { get; set; }
        public decimal ValorTotalEstoque { get; set; }
        public int MovimentacoesHoje { get; set; }

        public List<Estoque> ProdutosBaixoEstoque { get; set; }
        public List<MovimentacaoEstoque> UltimasMovimentacoes { get; set; }

        // Gráficos
        public Dictionary<string, decimal> ValorPorCategoria { get; set; }
        public Dictionary<string, int> MovimentacoesPorTipo { get; set; }
    }

    // ViewModels/Estoque/InventarioViewModel.cs
    public class InventarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(100, ErrorMessage = "A descrição deve ter até 100 caracteres")]
        public string Descricao { get; set; }

        [StringLength(500)]
        public string Observacao { get; set; }

        public List<ItemInventarioViewModel> Itens { get; set; } = new List<ItemInventarioViewModel>();
    }

    // ViewModels/Estoque/ItemInventarioViewModel.cs
    public class ItemInventarioViewModel
    {
        public int Id { get; set; }
        public int EstoqueId { get; set; }
        public string ProdutoNome { get; set; }
        public string UnidadeMedida { get; set; }
        public decimal QuantidadeSistema { get; set; }
        public decimal QuantidadeFisica { get; set; }
        public decimal Diferenca { get; set; }
        public decimal CustoUnitario { get; set; }
        public bool Conferido { get; set; }
        public string Observacao { get; set; }
    }

    public class DetailsProdutoViewModel
    {
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
        public decimal CustoUnitario { get; set; }

        [Required]
        public StatusUnidadeMedida StatusUnidadeMedida { get; set; }

        [Required]
        public StatusEstoque Status { get; set; }

        [Display(Name = "Categoria")]
        public int? CategoriaEstoqueId { get; set; }

        [Display(Name = "Fornecedor")]
        public int? FornecedorId { get; set; }
        public string Categoria { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public string NomeFornecedor { get; set; }
     
    }
}
