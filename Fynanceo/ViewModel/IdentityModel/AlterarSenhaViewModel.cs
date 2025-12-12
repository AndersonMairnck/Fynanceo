using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.IdentityModel;

public class AlterarSenhaViewModel
{
    [Required(ErrorMessage = "A senha atual é obrigatória")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha Atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova Senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Nova Senha")]
    [Compare("NovaSenha", ErrorMessage = "As senhas não conferem")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}