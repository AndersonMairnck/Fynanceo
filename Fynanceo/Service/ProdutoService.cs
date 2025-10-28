using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Fynanceo.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly AppDbContext _context;

        public ProdutoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Produto>> ObterTodosAsync()
        {
            return await _context.Produtos
                .Include(p => p.Ingredientes)
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<Produto> ObterPorIdAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.Ingredientes)
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
                    //MotivoIndisponibilidade = model.MotivoIndisponibilidade,
                    //CodigoNCM = model.CodigoNCM,
                    //Origem = model.Origem,
                    //CST = model.CST,
                    //Aliquota = model.Aliquota,
                    DataCadastro = DateTime.Now
                };

                // Adicionar ingredientes
                foreach (var ingredienteModel in model.Ingredientes.Where(i => !string.IsNullOrEmpty(i.Nome)))
                {
                    produto.Ingredientes.Add(new IngredienteProduto
                    {
                        Nome = ingredienteModel.Nome,
                        Quantidade = ingredienteModel.Quantidade,
                        UnidadeMedida = ingredienteModel.UnidadeMedida
                    });
                }

                _context.Produtos.Add(produto);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log do erro
                return false;
            }
        }

        public async Task<bool> AtualizarAsync(int id, ProdutoViewModel model)
        {
            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.Ingredientes)
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
                //produto.MotivoIndisponibilidade = model.MotivoIndisponibilidade;
                //produto.CodigoNCM = model.CodigoNCM;
                //produto.Origem = model.Origem;
                //produto.CST = model.CST;
                //produto.Aliquota = model.Aliquota;

                // Atualizar ingredientes
                _context.IngredientesProdutos.RemoveRange(produto.Ingredientes);

                foreach (var ingredienteModel in model.Ingredientes.Where(i => !string.IsNullOrEmpty(i.Nome)))
                {
                    produto.Ingredientes.Add(new IngredienteProduto
                    {
                        Nome = ingredienteModel.Nome,
                        Quantidade = ingredienteModel.Quantidade,
                        UnidadeMedida = ingredienteModel.UnidadeMedida
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
                var produto = await _context.Produtos.FindAsync(id);
                if (produto == null) return false;

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
                return true;
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
    }
}