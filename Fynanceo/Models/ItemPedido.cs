// Models/ItemPedido.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fynanceo.Models
{
    public class ItemPedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }


        [Required]
        public int ProdutoId { get; set; }
    
        public Produto Produto { get; set; }

        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(0.01, 100, ErrorMessage = "Quantidade deve ser entre 0.01 e 100")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total => Quantidade * PrecoUnitario;

        public string? Observacoes { get; set; }
        public string? Personalizacoes { get; set; }

        // Status do item
        public bool EnviadoCozinha { get; set; } = false;
        public bool EmPreparo { get; set; } = false;
        public bool Pronto { get; set; } = false;
        public bool Entregue { get; set; } = false;

        public DateTime? DataEnvioCozinha { get; set; }
        public DateTime? DataInicioPreparo { get; set; }
        public DateTime? DataPronto { get; set; }
        public DateTime? DataEntrega { get; set; }
    }
}