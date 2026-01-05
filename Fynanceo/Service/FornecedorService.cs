// Services/FornecedorService.cs

using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.FornecedorModel;
using Fynanceo.Utils;

namespace Fynanceo.Service
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
            fornecedor.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Fornecedor> obterFornecedorPorId(int fornecedorId)
        {
            try
            {
              var fornecedor = await _context.Fornecedores
                .Include(f => f.Contas)
                .FirstOrDefaultAsync(f => f.Id == fornecedorId);
               return (fornecedor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
         
        }

        public async Task<bool> AdicionarAsync(FornecerdorViewModel viewModel)
        {
            try
            {
                var fornercedores = new Fornecedor()
                {
                    Nome = viewModel.Nome,
                    CpfCnpj = StringUtils.RemoverCaracteresEspeciais(viewModel.CpfCnpj),
                    Telefone = StringUtils.RemoverCaracteresEspeciais(viewModel.Telefone),
                    Email = viewModel.Email,
                    Endereco = viewModel.Endereco,
                    Contato = viewModel.Contato,
                    Observacoes = viewModel.Observacoes,
                    DataCriacao = DateTime.UtcNow,
                    Status = viewModel.Status,

                };
                
                
               _context.Fornecedores.Add(fornercedores);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            
        }

        public async Task<bool> EditarFornecedor(int id, EditarFornecedorViewModel viewModel)
        {
            try
            {
                var fornecedor = await _context.Fornecedores
                   .FirstOrDefaultAsync(c => c.Id == id);
                
                if (fornecedor == null) return false;


                fornecedor.Nome = viewModel.Nome;
                fornecedor.CpfCnpj = StringUtils.RemoverCaracteresEspeciais(viewModel.CpfCnpj);
                fornecedor.Telefone = StringUtils.RemoverCaracteresEspeciais(viewModel.Telefone);
                fornecedor.Email = viewModel.Email;
                fornecedor.Endereco = viewModel.Endereco;
                fornecedor.Contato = viewModel.Contato;
                fornecedor.Observacoes = viewModel.Observacoes;
                fornecedor.DataCriacao = DateTime.UtcNow;
                fornecedor.Status = viewModel.Status;

                
                    await _context.SaveChangesAsync();
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // Busca fornecedores por nome (autocomplete). Case-insensitive, usa AsNoTracking e limita resultados.
        public async Task<List<Fornecedor>> BuscarFornecedoresAsync(string query, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 1)
                return new List<Fornecedor>();

            var q = query.Trim().ToLower();
            var max = Math.Min(Math.Max(limit, 1), 50); // cap between 1 and 50

            return await _context.Fornecedores
                .AsNoTracking()
                .Where(f => f.Nome != null && f.Nome.ToLower().Contains(q))
                .OrderBy(f => f.Nome)
                .Select(f => new Fornecedor { Id = f.Id, Nome = f.Nome })
                .Take(max)
                .ToListAsync();
        }

        public async Task<(List<Fornecedor> Itens, int Total)> BuscarFornecedoresPaginadosAsync(string searchTerm, int page, int pageSize = 20)
        {
            if (page < 1) page = 1;
            var size = Math.Min(Math.Max(pageSize, 1), 50);

            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Trim().Length < 3)
                return (new List<Fornecedor>(), 0);

            var term = searchTerm.Trim();

            var query = _context.Fornecedores
                .AsNoTracking()
                .Where(f => f.Status == StatusFornecedor.Ativo)
                .Where(f => f.Nome != null && EF.Functions.ILike(f.Nome, $"%{term}%"))
                .Select(f => new Fornecedor
                {
                    Id = f.Id,
                    Nome = f.Nome
                });;

            var total = await query.CountAsync();

            var itens = await query
                .OrderBy(f => f.Nome)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(f => new Fornecedor { Id = f.Id, Nome = f.Nome })
                .ToListAsync();

            return (itens, total);
        }
    }
}
