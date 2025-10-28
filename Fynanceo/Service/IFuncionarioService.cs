using Fynanceo.Models;
using Fynanceo.ViewModels;

namespace Fynanceo.Services
{
    public interface IFuncionarioService
    {
        Task<List<Funcionario>> ObterTodosAsync();
        Task<Funcionario> ObterPorIdAsync(int id);
        Task<bool> AdicionarAsync(FuncionarioViewModel model);
        Task<bool> AtualizarAsync(int id, FuncionarioViewModel model);
        Task<bool> ExcluirAsync(int id);
        Task<bool> CpfExisteAsync(string cpf, int? id = null);
        Task<List<string>> ObterCargosAsync();
    }
}