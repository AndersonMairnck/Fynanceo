using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.ProdutosModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text;
using Fynanceo.Models.Enums;

namespace Fynanceo.Service
{
    public class ProdutoService : IProdutoService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IPedidoService _pedidoService;

        public ProdutoService(AppDbContext context, IMemoryCache memoryCache, IPedidoService pedidoService)
        {
            _context = context;
            _cache = memoryCache;
            _pedidoService = pedidoService;

        }

        public async Task<List<Produto>> ObterTodosAsync()
        {
            return await _context.Produtos
                .Include(p => p.ProdutoIngredientes)
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<Produto> ObterPorIdAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.ProdutoIngredientes)
                .ThenInclude(i => i.Estoque)
              
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> AdicionarAsync(ProdutoViewModel model)
        {
            try
            {
                var produto = new Produto
                {
                    Codigo = model.Codigo,
                    Nome = model.Nome,
                    Descricao = model.Descricao,
                    Categoria = model.Categoria,
                    Subcategoria = model.Subcategoria,
                    CustoUnitario = model.CustoUnitario,
                    ValorVenda = model.ValorVenda,
                    TempoPreparoMinutos = model.TempoPreparoMinutos,
                    TempoExtraPico = model.TempoExtraPico,
                    OpcoesPersonalizacao = model.OpcoesPersonalizacao,
                    Disponivel = model.Disponivel,

                    DataCadastro = DateTime.UtcNow,

                };

                // Adicionar ingredientes
                foreach (var ingredienteModel in model.Ingredientes.Where(i => !string.IsNullOrEmpty(i.Nome)))
                {
                    produto.ProdutoIngredientes.Add(new ProdutoIngrediente
                    {
                       EstoqueId = ingredienteModel.IdEstoque,
                        Quantidade = ingredienteModel.Quantidade,
                       UnidadeMedida = ingredienteModel.UnidadeMedida,
                         ProdutoId = ingredienteModel.Id,
                          
                        
                         Observacao= ingredienteModel.Observacao,
                       

                    });
                }

               _context.Produtos.Add(produto);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public async Task<bool> AtualizarAsync(int id, ProdutoViewModel model)
        {
            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.ProdutoIngredientes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (produto == null) return false;

                // Atualizar dados básicos
                produto.Codigo = model.Codigo;
                produto.Nome = model.Nome;
                produto.Descricao = model.Descricao;
                produto.Categoria = model.Categoria;
                produto.Subcategoria = model.Subcategoria;
                produto.CustoUnitario = model.CustoUnitario;
                produto.ValorVenda = model.ValorVenda;
                produto.TempoPreparoMinutos = model.TempoPreparoMinutos;
                produto.TempoExtraPico = model.TempoExtraPico;
                produto.OpcoesPersonalizacao = model.OpcoesPersonalizacao;
                produto.Disponivel = model.Disponivel;
          

                // Atualizar ingredientes
                _context.ProdutoIngredientes.RemoveRange(produto.ProdutoIngredientes);

                foreach (var ingredienteModel in model.Ingredientes.Where(i => !string.IsNullOrEmpty(i.Nome)))
                {
                    produto.ProdutoIngredientes.Add(new ProdutoIngrediente()
                    {
                        EstoqueId = ingredienteModel.IdEstoque,
                        Quantidade = ingredienteModel.Quantidade,
                        UnidadeMedida = ingredienteModel.UnidadeMedida,
                        ProdutoId = ingredienteModel.Id,
                        Observacao= ingredienteModel.Observacao,
                        
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
                var temvenda = await _pedidoService.VerificaProdutoJaVendido(id);

                if (temvenda == true)
                {
                    return false;
                }
                else
                {
                      var produto = await _context.Produtos.FindAsync(id);
                                    if (produto == null) return false;
                    
                                    _context.Produtos.Remove(produto);
                                    await _context.SaveChangesAsync();
                                    return true;
                }
                
              
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CodigoExisteAsync(string codigo, int? id = null)
        {
            return await _context.Produtos
                .AnyAsync(p => p.Codigo == codigo && (!id.HasValue || p.Id != id.Value));
        }

        public async Task<List<string>> ObterCategoriasAsync()
        {
            return await _context.Produtos
                .Select(p => p.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> ObterSubcategoriasAsync()
        {
            return await _context.Produtos
                .Where(p => !string.IsNullOrEmpty(p.Subcategoria))
                .Select(p => p.Subcategoria)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }
        public async Task<List<Produto>> ObterProdutosPopularesAsync(int quantidade)
        {
            var cacheKey = $"produtos_populares_{quantidade}";

            if (!_cache.TryGetValue(cacheKey, out List<Produto> produtos))
            {
                produtos = await _context.Produtos
                    .Where(p => p.Disponivel)
                    .OrderBy(p => p.Nome) // Ordena por nome como fallback
                    .Take(quantidade)
                    .ToListAsync();

                _cache.Set(cacheKey, produtos, TimeSpan.FromMinutes(30));
            }

            return produtos;
        }

        public async Task<List<Produto>> ObterPorCategoriaAsync(string categoria)
        {
            return await _context.Produtos
                .Where(p => p.Disponivel && p.Categoria == categoria)
                .OrderBy(p => p.Nome)
                .Take(50)
                .ToListAsync();
        }

        public async Task<(List<Produto> Produtos, int TotalCount)> ObterProdutosPaginadosAsync(int page, int pageSize, string search = "", string categoria = "")
        {
            var query = _context.Produtos.Where(p => p.Disponivel);

            // Filtro por categoria (corrigido)
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria == categoria);
            }

            // Filtro por busca (case insensitive)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower(); // Converte para minúsculas para busca case insensitive
                query = query.Where(p =>
                    p.Nome.ToLower().Contains(search) ||
                    (p.Descricao != null && p.Descricao.ToLower().Contains(search)) ||
                    p.Categoria.ToLower().Contains(search) ||
                    (p.Subcategoria != null && p.Subcategoria.ToLower().Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var produtos = await query
                .OrderBy(p => p.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (produtos, totalCount);
        }

        // No ProdutoService
        // No ProdutoService - Versão Cliente
        public async Task<List<Produto>> BuscarProdutosAsync(string termo, string categoria)
        {
            var query = _context.Produtos
                .Where(p => p.Disponivel)
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria == categoria);
            }

            // Busca todos os produtos (limitado) e filtra no lado do cliente
            var produtos = await query
                .OrderBy(p => p.Nome)
                .Take(200) // Limite maior para garantir resultados
                .ToListAsync();

            // Se não há termo de busca, retorna os produtos
            if (string.IsNullOrEmpty(termo))
            {
                return produtos.Take(50).ToList();
            }

            // Filtra no lado do cliente (case-insensitive e sem acentos)
            termo = termo.ToLower().Trim();
            var termoSemAcentos = RemoverAcentos(termo);

            var produtosFiltrados = produtos.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                RemoverAcentos(p.Nome.ToLower()).Contains(termoSemAcentos) ||
                (p.Descricao != null && (
                    p.Descricao.ToLower().Contains(termo) ||
                    RemoverAcentos(p.Descricao.ToLower()).Contains(termoSemAcentos)
                ))
            ).Take(50).ToList();

            return produtosFiltrados;
        }

        // Método otimizado para remover acentos
        private static string RemoverAcentos(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }

    }
}
