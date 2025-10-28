using Fynanceo.Models;
using Fynanceo.ViewModels;

namespace Fynanceo.Services
{
    public interface IProdutoService
    {
        Task<List<Produto>> ObterTodosAsync();
        Task<Produto> ObterPorIdAsync(int id);
        Task<bool> AdicionarAsync(ProdutoViewModel model);
        Task<bool> AtualizarAsync(int id, ProdutoViewModel model);
        Task<bool> ExcluirAsync(int id);
        Task<bool> CodigoExisteAsync(string codigo, int? id = null);
        Task<List<string>> ObterCategoriasAsync();
        Task<List<string>> ObterSubcategoriasAsync();
    }
}