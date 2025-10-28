using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.ViewModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Fynanceo.Utils;

namespace Fynanceo.Services
{
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;

        public ClienteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> ObterTodosAsync()
        {
            return await _context.Clientes
                .Include(c => c.Enderecos)
                .OrderBy(c => c.NomeCompleto)
                .ToListAsync();
        }

        public async Task<Cliente> ObterPorIdAsync(int id)
        {
            return await _context.Clientes
                .Include(c => c.Enderecos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AdicionarAsync(ClienteViewModel model)
        {
            try
            {
                var cliente = new Cliente
                {
                    NomeCompleto = model.NomeCompleto,
                    CpfCnpj = StringUtils.RemoverCaracteresEspeciais(model.CpfCnpj),
                    Telefone = StringUtils.RemoverCaracteresEspeciais(model.Telefone),
                    Email = model.Email,
                    DataNascimento = model.DataNascimento,
                    Classificacao = model.Classificacao,
                    Observacoes = model.Observacoes,
                    Ativo = model.Ativo,
                    JustificativaStatus = model.JustificativaStatus,
                    DataCadastro = DateTime.UtcNow
                };

                // Adicionar endereço principal se informado
                if (!string.IsNullOrEmpty(model.Logradouro))
                {
                    cliente.Enderecos.Add(new EnderecoCliente
                    {
                        Logradouro = model.Logradouro,
                        Numero = model.Numero,
                        Complemento = model.Complemento,
                        Bairro = model.Bairro,
                        Cidade = model.Cidade,
                        Estado = model.Estado,
                        Cep = StringUtils.RemoverCaracteresEspeciais(model.Cep),
                        Referencia = model.Referencia,
                        Principal = true
                    });
                }

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "22001")
            {
                // Captura qual campo causou o estouro de tamanho
                foreach (var entry in ex.Entries)
                {
                    foreach (var prop in entry.CurrentValues.Properties)
                    {
                        var value = entry.CurrentValues[prop];
                        if (value is string s)
                        {
                            var maxLength = GetMaxLength(entry, prop);
                            if (s.Length > maxLength)
                            {
                                throw new Exception($"O campo '{prop.Name}' excedeu o tamanho máximo ({maxLength}). Valor: '{s}'");
                            }
                        }
                    }
                }

                throw; // se não encontrou, relança a exceção
            }
        }

        // Método auxiliar para obter o tamanho máximo definido no EF Core
        private int GetMaxLength(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, Microsoft.EntityFrameworkCore.Metadata.IProperty prop)
        {
            var maxLength = prop.GetMaxLength();
            // Se não estiver definido, assume um valor grande para não gerar falso positivo
            return maxLength ?? int.MaxValue;
        }

        public async Task<bool> AtualizarAsync(int id, ClienteViewModel model)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Enderecos)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cliente == null) return false;

                cliente.NomeCompleto = model.NomeCompleto;
                cliente.CpfCnpj = model.CpfCnpj;
                cliente.Telefone = model.Telefone;
                cliente.Email = model.Email;
                cliente.DataNascimento = model.DataNascimento;
                cliente.Classificacao = model.Classificacao;
                cliente.Observacoes = model.Observacoes;
                cliente.Ativo = model.Ativo;
                cliente.JustificativaStatus = model.JustificativaStatus;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null) return false;

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CpfCnpjExisteAsync(string cpfCnpj, int? id = null)
        {
            return await _context.Clientes
                .AnyAsync(c => c.CpfCnpj == cpfCnpj && (!id.HasValue || c.Id != id.Value));
        }
    }
}