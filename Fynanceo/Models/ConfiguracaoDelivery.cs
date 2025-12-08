// Models/ConfiguracaoDelivery.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fynanceo.Models
{
    public class ConfiguracaoDelivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxaBase { get; set; } = 5.00m;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorMinimoGratis { get; set; } = 30.00m;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ComissaoBase { get; set; } = 3.00m;

        [Required]
        public int RaioMaximoEntrega { get; set; } = 10; // km

        [Required]
        public int TempoEstimadoBase { get; set; } = 30; // minutos

        public string? RegioesCobertas { get; set; } // JSON com regiões e taxas
        public string? HorariosFuncionamento { get; set; } // JSON com horários

        public bool Ativo { get; set; } = true;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }
}