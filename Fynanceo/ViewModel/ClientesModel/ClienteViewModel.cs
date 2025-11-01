using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.ClientesModel
{
    public class ClienteViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }

        [Display(Name = "CPF/CNPJ")]
        public string CpfCnpj { get; set; }

        [Display(Name = "Telefone")]
        public string Telefone { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        [Display(Name = "Classificação")]
        public string Classificacao { get; set; } = "Normal";

        [Display(Name = "Observações")]
        public string Observacoes { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Justificativa do Status")]
        public string? JustificativaStatus { get; set; }

        // Endereço Principal
        [Display(Name = "Logradouro")]
        public string Logradouro { get; set; }

        [Display(Name = "Número")]
        public string Numero { get; set; }

        [Display(Name = "Complemento")]
        public string Complemento { get; set; }

        [Display(Name = "Bairro")]
        public string Bairro { get; set; }

        [Display(Name = "Cidade")]
        public string Cidade { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "CEP")]
        public string Cep { get; set; }

        [Display(Name = "Referência")]
        public string Referencia { get; set; }
    }
}