// Models/Enums/StatusFornecedor.cs
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public enum StatusFornecedor
    {
        [Display(Name = "Ativo")]
        Ativo = 1,

        [Display(Name = "Inativo")]
        Inativo = 0,

        [Display(Name = "Bloqueado")]
        Bloqueado = 2,

        [Display(Name = "Pendente")]
        Pendente = 3
    }
}