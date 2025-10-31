// Services/IFornecedorService.cs
using Fynanceo.Models;

namespace Fynanceo.Services
{
    public interface IFornecedorService
    {
        Task<List<Fornecedor>> ObterFornecedoresAtivosAsync();
        Task<List<Fornecedor>> ObterFornecedoresPorStatusAsync(StatusFornecedor status);
        Task<List<Fornecedor>> ObterTodosFornecedoresAsync(); // ADICIONADO ESTE MÉTODO
        Task<bool> AlterarStatusFornecedorAsync(int fornecedorId, StatusFornecedor novoStatus);
    }
}