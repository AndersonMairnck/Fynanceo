using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.MesasModel;
using Microsoft.EntityFrameworkCore;

namespace Fynanceo.Services
{
    public class MesaService : IMesaService
    {
        private readonly AppDbContext _context;

        public MesaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Mesa>> ObterTodosAsync()
        {
            return await _context.Mesas
                .OrderBy(m => m.Numero)
                .ToListAsync();
        }

        public async Task<Mesa> ObterPorIdAsync(int id)
        {
            return await _context.Mesas.FindAsync(id);
        }

        public async Task<bool> AdicionarAsync(MesaViewModel model)
        {
            try
            {
                var mesa = new Mesa
                {
                    Numero = model.Numero,
                    Capacidade = model.Capacidade,
                    Localizacao = model.Localizacao,
                    Ambiente = model.Ambiente,
                    Status = model.Status,
                    Descricao = model.Descricao,
                    DataCadastro = DateTime.Now
                };

                _context.Mesas.Add(mesa);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AtualizarAsync(int id, MesaViewModel model)
        {
            try
            {
                var mesa = await _context.Mesas.FindAsync(id);
                if (mesa == null) return false;

                mesa.Numero = model.Numero;
                mesa.Capacidade = model.Capacidade;
                mesa.Localizacao = model.Localizacao;
                mesa.Ambiente = model.Ambiente;
                mesa.Status = model.Status;
                mesa.Descricao = model.Descricao;

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
                var mesa = await _context.Mesas.FindAsync(id);
                if (mesa == null) return false;

                _context.Mesas.Remove(mesa);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> NumeroExisteAsync(string numero, int? id = null)
        {
            return await _context.Mesas
                .AnyAsync(m => m.Numero == numero && (!id.HasValue || m.Id != id.Value));
        }

        public async Task<bool> AtualizarStatusAsync(int id, string status)
        {
            try
            {
                var mesa = await _context.Mesas.FindAsync(id);
                if (mesa == null) return false;

                mesa.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Mesa>> ObterPorStatusAsync(string status)
        {
            return await _context.Mesas
                .Where(m => m.Status == status)
                .OrderBy(m => m.Numero)
                .ToListAsync();
        }
    }
}