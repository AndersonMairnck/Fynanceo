using Fynanceo.Models;
using Fynanceo.ViewModel.EstoquesModel;

namespace Fynanceo.Service.Interface
{
    public interface IConfigService
    {
        Task<CozinhaConfig> ObterConfigCozinhaAsync();
        Task AtualizarConfigCozinhaAsync(CozinhaConfig config);
    }
}
