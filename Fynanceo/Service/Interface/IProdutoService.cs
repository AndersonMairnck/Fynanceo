
using Fynanceo.Models;
using Fynanceo.ViewModel.ProdutosModel;

namespace Fynanceo.Service.Interface
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
        Task<List<Produto>> ObterProdutosPopularesAsync(int quantidade);
        Task<List<Produto>> ObterPorCategoriaAsync(string categoria);
        Task<(List<Produto> Produtos, int TotalCount)> ObterProdutosPaginadosAsync(int page, int pageSize, string search = "",string categoria="");
        Task<List<Produto>> BuscarProdutosAsync(string termo, string categoria);
    }
}