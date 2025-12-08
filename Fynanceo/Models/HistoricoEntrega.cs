// Models/HistoricoEntrega.cs
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class HistoricoEntrega
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EntregaId { get; set; }
        public Entrega Entrega { get; set; }

        [Required]
        public string StatusAnterior { get; set; }

        [Required]
        public string StatusNovo { get; set; }

        public string? Observacao { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
    }
}