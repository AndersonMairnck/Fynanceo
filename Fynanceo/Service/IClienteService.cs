using Fynanceo.Models;
using Fynanceo.ViewModels;

namespace Fynanceo.Services
{
    public interface IClienteService
    {
        Task<List<Cliente>> ObterTodosAsync();
        Task<Cliente> ObterPorIdAsync(int id);
        Task<bool> AdicionarAsync(ClienteViewModel model);
        Task<bool> AtualizarAsync(int id, ClienteViewModel model);
        Task<bool> ExcluirAsync(int id);
        Task<bool> CpfCnpjExisteAsync(string cpfCnpj, int? id = null);
    }
}