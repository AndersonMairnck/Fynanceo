using Fynanceo.Data;
using Fynanceo.Service.Interface;
using Fynanceo.Models;

namespace Fynanceo.Service
{
    public class ConfigService : IConfigService
    {
        private readonly AppDbContext _context;

        public ConfigService(AppDbContext context)
        {
            _context = context;
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
            config.Id = 1; // Garante que sempre atualiza o registro com ID 1
            config.DataAtualizacao = DateTime.UtcNow;

            _context.CozinhaConfigs.Update(config);
            await _context.SaveChangesAsync();
        }
    }
}
