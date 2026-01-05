// Services/IFornecedorService.cs

// Services/IFornecedorService.cs
using Fynanceo.Models;
using Fynanceo.ViewModel.FornecedorModel;
namespace Fynanceo.Service.Interface
{
    public interface IFornecedorService
    {
        Task<List<Fornecedor>> ObterFornecedoresAtivosAsync();
        Task<List<Fornecedor>> ObterFornecedoresPorStatusAsync(StatusFornecedor status);
        Task<List<Fornecedor>> ObterTodosFornecedoresAsync(); // ADICIONADO ESTE MÉTODO
        // Busca fornecedores por texto (usado por autocomplete). Retorna lista limitada.
        Task<List<Fornecedor>> BuscarFornecedoresAsync(string query, int limit = 20);
        Task<(List<Fornecedor> Itens, int Total)> BuscarFornecedoresPaginadosAsync(string searchTerm, int page, int pageSize = 20);
        Task<bool> AlterarStatusFornecedorAsync(int fornecedorId, StatusFornecedor novoStatus);
        Task<Fornecedor> obterFornecedorPorId(int fornecedorId);
        Task<bool> AdicionarAsync(FornecerdorViewModel fornecedor);
        Task<bool> EditarFornecedor(int id, EditarFornecedorViewModel fornecedor);
        
    }
}