using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class Funcionario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome completo é obrigatório")]
        public string NomeCompleto { get; set; }

        

        [Required]
        public string CPF { get; set; }

        public string Endereco { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string ContatoEmergencia { get; set; }
        public string TelefoneEmergencia { get; set; }

        [Required]
        public string Cargo { get; set; } // Garçom, Cozinheiro, Caixa, Gerente

        public string NivelPermissao { get; set; } // Basico, Medio, Alto
        public string? NomeFoto { get; set; }
        public string Turno { get; set; } // Manhã, Tarde, Noite, Integral
        public bool Ativo { get; set; } = true;
        public DateTime DataAdmissao { get; set; } = DateTime.Now;
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}