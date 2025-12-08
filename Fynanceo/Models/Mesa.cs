using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class Mesa
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Número da mesa é obrigatório")]
        public string Numero { get; set; }

        [Range(1, 50, ErrorMessage = "Capacidade deve ser entre 1 e 50 pessoas")]
        public int Capacidade { get; set; }

        [Required]
        public string Localizacao { get; set; } // Interna, Externa, Varanda

        public string Ambiente { get; set; } // Salão Principal, Área VIP, etc.

        public string Status { get; set; } = "Livre"; // Livre, Ocupada, Reservada, EmLimpeza

        public string Descricao { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}