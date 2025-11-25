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
        Task<bool> AlterarStatusFornecedorAsync(int fornecedorId, StatusFornecedor novoStatus);
        Task<Fornecedor> obterFornecedorPorId(int fornecedorId);
        Task<bool> AdicionarAsync(FornecerdorViewModel fornecedor);
        Task<bool> EditarFornecedor(int id, EditarFornecedorViewModel fornecedor);
        
    }
}