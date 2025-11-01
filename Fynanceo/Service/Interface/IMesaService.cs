using Fynanceo.Models;
using Fynanceo.ViewModel.MesasModel;

namespace Fynanceo.Service.Interface
{
    public interface IMesaService
    {
        Task<List<Mesa>> ObterTodosAsync();
        Task<Mesa> ObterPorIdAsync(int id);
        Task<bool> AdicionarAsync(MesaViewModel model);
        Task<bool> AtualizarAsync(int id, MesaViewModel model);
        Task<bool> ExcluirAsync(int id);
        Task<bool> NumeroExisteAsync(string numero, int? id = null);
        Task<bool> AtualizarStatusAsync(int id, string status);
        Task<List<Mesa>> ObterPorStatusAsync(string status);
    }
}