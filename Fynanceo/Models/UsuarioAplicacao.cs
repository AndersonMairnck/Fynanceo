using Microsoft.AspNetCore.Identity;

namespace Fynanceo.Models
{
    public class UsuarioAplicacao : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string? Cargo { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
     
        // ✅ Propriedade para controlar sessão única
        public string? CurrentSessionId { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}