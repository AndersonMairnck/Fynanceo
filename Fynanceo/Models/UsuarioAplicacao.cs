using Microsoft.AspNetCore.Identity;

namespace Fynanceo.Models
{
    public class UsuarioAplicacao : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string? Cargo { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
    }
}