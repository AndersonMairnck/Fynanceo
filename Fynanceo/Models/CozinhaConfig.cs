// Models/CozinhaConfig.cs
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class CozinhaConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tempo Alerta Preparo (minutos)")]
        public int TempoAlertaPreparoMinutos { get; set; } = 30;

        [Required]
        [Display(Name = "Tempo Alerta Pronto (minutos)")]
        public int TempoAlertaProntoMinutos { get; set; } = 10;

        [Required]
        [Display(Name = "Intervalo Atualização (segundos)")]
        public int IntervaloAtualizacaoSegundos { get; set; } = 30;

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public string? UsuarioAtualizacao { get; set; }
    }
}