using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
        public string NomeCompleto { get; set; }

        [StringLength(14, ErrorMessage = "CPF/CNPJ não pode exceder 14 caracteres")]
        public string CpfCnpj { get; set; }

        [Phone(ErrorMessage = "Telefone inválido")]
        public string Telefone { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        public string Classificacao { get; set; } = "Normal"; // VIP, Frequente, Normal
        public string Observacoes { get; set; }
        public bool Ativo { get; set; } = true;
        public string? JustificativaStatus { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Navegação
        public virtual ICollection<EnderecoCliente> Enderecos { get; set; } = new List<EnderecoCliente>();
    }

    public class EnderecoCliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Logradouro { get; set; }

        [Required]
        public string Numero { get; set; }

        public string Complemento { get; set; }

        [Required]
        public string Bairro { get; set; }

        [Required]
        public string Cidade { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string Estado { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 8)]
        public string Cep { get; set; }

        public string Referencia { get; set; }
        public bool Principal { get; set; }

        // Foreign Key
        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; }
    }
}