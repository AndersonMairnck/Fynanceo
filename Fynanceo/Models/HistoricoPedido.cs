// Models/HistoricoPedido.cs
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class HistoricoPedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        [Required]
        public string StatusAnterior { get; set; }

        [Required]
        public string StatusNovo { get; set; }

        public string? Observacao { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }

        public DateTime DataAlteracao { get; set; } = DateTime.Now;
    }
}