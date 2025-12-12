using Fynanceo.Data;
using Fynanceo.Service.Interface;
using Fynanceo.Models;
using Microsoft.AspNetCore.Identity;

namespace Fynanceo.Service
{
    public class ConfigService : IConfigService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<UsuarioAplicacao> _userManager;

        public ConfigService(AppDbContext context,  
            IHttpContextAccessor httpContextAccessor,
            UserManager<UsuarioAplicacao> userManager)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CozinhaConfig> ObterConfigCozinhaAsync()
        {
            // Sempre retorna a configuração com ID 1
            var config = await _context.CozinhaConfigs.FindAsync(1);

            if (config == null)
            {
                // Se não existir, cria uma padrão
                config = new CozinhaConfig { Id = 1 };
                _context.CozinhaConfigs.Add(config);
                await _context.SaveChangesAsync();
            }

            return config;
        }

        public async Task AtualizarConfigCozinhaAsync(CozinhaConfig config)
        {
            var usuario = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            config.Id = 1; // Garante que sempre atualiza o registro com ID 1
            config.DataAtualizacao = DateTime.UtcNow;
            config.UsuarioAtualizacao = usuario.UserName;

            _context.CozinhaConfigs.Update(config);
            await _context.SaveChangesAsync();
        }
    }
}
