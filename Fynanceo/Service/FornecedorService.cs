// Services/FornecedorService.cs
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Service.Interface;

namespace Fynanceo.Services
{
    public class FornecedorService : IFornecedorService
    {
        private readonly AppDbContext _context;

        public FornecedorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Fornecedor>> ObterTodosFornecedoresAsync()
        {
            return await _context.Fornecedores
                .OrderBy(f => f.Nome)
                .ToListAsync();
        }

        public async Task<List<Fornecedor>> ObterFornecedoresAtivosAsync()
        {
            return await _context.Fornecedores
                .Where(f => f.Status == StatusFornecedor.Ativo)
                .OrderBy(f => f.Nome)
                .ToListAsync();
        }

        public async Task<List<Fornecedor>> ObterFornecedoresPorStatusAsync(StatusFornecedor status)
        {
            return await _context.Fornecedores
                .Where(f => f.Status == status)
                .OrderBy(f => f.Nome)
                .ToListAsync();
        }

        public async Task<bool> AlterarStatusFornecedorAsync(int fornecedorId, StatusFornecedor novoStatus)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(fornecedorId);
            if (fornecedor == null) return false;

            fornecedor.Status = novoStatus;
            fornecedor.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}