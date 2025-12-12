using Fynanceo.Data;
using Fynanceo.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Fynanceo.Utils;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.ClientesModel;
using Microsoft.AspNetCore.Identity;
namespace Fynanceo.Service
{
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UsuarioAplicacao> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public ClienteService(AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<UsuarioAplicacao> userManager)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Cliente>> ObterTodosAsync()
        {
            return await _context.Clientes
                .Include(c => c.Enderecos)
                .OrderBy(c => c.NomeCompleto)
                .ToListAsync();
        }

        public async Task<Cliente?> ObterPorIdAsync(int id)
        {
            return await _context.Clientes
                .Include(c => c.Enderecos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AdicionarAsync(ClienteViewModel model)
        {
            var usuario = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
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
                    DataCadastro = DateTime.UtcNow,
                    UsuarioNome = usuario.UserName,
                };

                // Adicionar endereço principal se informado
                if (model.Enderecos != null && model.Enderecos.Any())
                {
                    // mapear todos os endereços enviados (preserva Principal quando marcado)
                    foreach (var e in model.Enderecos)
                    {
                        // pular endereços vazios
                        if (string.IsNullOrWhiteSpace(e.Logradouro) && string.IsNullOrWhiteSpace(e.Cidade) && string.IsNullOrWhiteSpace(e.Bairro))
                            continue;

                        cliente.Enderecos.Add(new EnderecoCliente
                        {
                            Logradouro = e.Logradouro,
                            Numero = e.Numero,
                            Complemento = e.Complemento,
                            Bairro = e.Bairro,
                            Cidade = e.Cidade,
                            Estado = e.Estado,
                            Cep = StringUtils.RemoverCaracteresEspeciais(e.Cep),
                            Referencia = e.Referencia,
                            Principal = e.Principal
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(model.Logradouro))
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
                            var maxLength = GetMaxLength(prop);
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
        private int GetMaxLength(Microsoft.EntityFrameworkCore.Metadata.IProperty prop)
        {
            var maxLength = prop.GetMaxLength();
            // Se não estiver definido, assume um valor grande para não gerar falso positivo
            return maxLength ?? int.MaxValue;
        }

        public async Task<bool> AtualizarAsync(int id, ClienteViewModel model)
        {
            var usuario = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
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
                cliente.UsuarioNome = usuario.UserName;
                // Processar coleção de endereços (sincronizar)
                if (model.Enderecos != null)
                {
                    // Remover endereços que não estão na lista recebida
                    var idsRecebidos = model.Enderecos.Where(e => e.Id > 0).Select(e => e.Id).ToHashSet();
                    var toRemove = cliente.Enderecos.Where(e => !idsRecebidos.Contains(e.Id)).ToList();
                    foreach (var r in toRemove)
                    {
                        cliente.Enderecos.Remove(r);
                        _context.Entry(r).State = EntityState.Deleted;
                    }

                    // Atualizar ou adicionar
                    foreach (var evm in model.Enderecos)
                    {
                        // pular endereços vazios
                        if (string.IsNullOrWhiteSpace(evm.Logradouro) && string.IsNullOrWhiteSpace(evm.Cidade) && string.IsNullOrWhiteSpace(evm.Bairro))
                            continue;

                        if (evm.Id > 0)
                        {
                            var existing = cliente.Enderecos.FirstOrDefault(e => e.Id == evm.Id);
                            if (existing != null)
                            {
                                existing.Logradouro = evm.Logradouro;
                                existing.Numero = evm.Numero;
                                existing.Complemento = evm.Complemento;
                                existing.Bairro = evm.Bairro;
                                existing.Cidade = evm.Cidade;
                                existing.Estado = evm.Estado;
                                existing.Cep = evm.Cep;
                                existing.Referencia = evm.Referencia;
                                existing.Principal = evm.Principal;
                            }
                        }
                        else
                        {
                            cliente.Enderecos.Add(new EnderecoCliente
                            {
                                Logradouro = evm.Logradouro,
                                Numero = evm.Numero,
                                Complemento = evm.Complemento,
                                Bairro = evm.Bairro,
                                Cidade = evm.Cidade,
                                Estado = evm.Estado,
                                Cep = evm.Cep,
                                Referencia = evm.Referencia,
                                Principal = evm.Principal
                            });
                        }
                    }
                }
                else
                {
                    // Fallback: manter compatibilidade com campos individuais (adiciona novo endereço)
                    cliente.Enderecos.Add(new EnderecoCliente
                    {
                        Logradouro = model.Logradouro,
                        Numero = model.Numero,
                        Complemento = model.Complemento,
                        Bairro = model.Bairro,
                        Cidade = model.Cidade,
                        Estado = model.Estado,
                        Cep = model.Cep,
                        Referencia = model.Referencia,
                       
                    });
                }

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

        public async Task<(List<Cliente> Clientes, int TotalCount, int TotalPages)> ObterClientesPaginadosAsync(int page, int pageSize, string search = "")
        {
            var query = _context.Clientes.AsQueryable();

            // Filtro de busca (case insensitive)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    c.NomeCompleto.ToLower().Contains(search) ||
                    c.CpfCnpj.Contains(search) ||
                    c.Telefone.Contains(search) ||
                    c.Email.ToLower().Contains(search) ||
                    c.Classificacao.ToLower().Contains(search)
                );
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var clientes = await query
                .Include(c => c.Enderecos)
                .OrderBy(c => c.NomeCompleto)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (clientes, totalCount, totalPages);
        }

    }
}