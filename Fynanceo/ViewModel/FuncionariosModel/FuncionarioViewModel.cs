using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.FuncionariosModel
{
    public class FuncionarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }


        [Required(ErrorMessage = "CPF é obrigatório")]
        [Display(Name = "CPF")]
        public string CPF { get; set; }

        [Display(Name = "Endereço")]
        public string Endereco { get; set; }

        [Phone(ErrorMessage = "Telefone inválido")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Contato de Emergência")]
        public string ContatoEmergencia { get; set; }

        [Display(Name = "Telefone de Emergência")]
        public string TelefoneEmergencia { get; set; }

        [Required(ErrorMessage = "Cargo é obrigatório")]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; }

        [Required(ErrorMessage = "Nível de permissão é obrigatório")]
        [Display(Name = "Nível de Permissão")]
        public string NivelPermissao { get; set; }

        [Display(Name = "Turno")]
        public string Turno { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Required(ErrorMessage = "Data de admissão é obrigatória")]
        [Display(Name = "Data de Admissão")]
        [DataType(DataType.Date)]
        public DateTime DataAdmissao { get; set; } = DateTime.UtcNow;
    }
}