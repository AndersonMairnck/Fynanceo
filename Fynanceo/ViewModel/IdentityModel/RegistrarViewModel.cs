
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.IdentityModel;

public class RegistrarViewModel
{
 
        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "O cargo deve ter no máximo {1} caracteres")]
        [Display(Name = "Cargo")]
        public string? Cargo { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Senha", ErrorMessage = "As senhas não conferem")]
        public string ConfirmarSenha { get; set; } = string.Empty;

        [Display(Name = "Perfil")]
        public string? PerfilSelecionado { get; set; }
    }
