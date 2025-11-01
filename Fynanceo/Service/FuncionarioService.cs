using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Service.Interface;
using Fynanceo.Utils;
using Fynanceo.ViewModel.FuncionariosModel;
using Microsoft.EntityFrameworkCore;

namespace Fynanceo.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly AppDbContext _context;

        public FuncionarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Funcionario>> ObterTodosAsync()
        {
            return await _context.Funcionarios
                .OrderBy(f => f.NomeCompleto)
                .ToListAsync();
        }

        public async Task<Funcionario> ObterPorIdAsync(int id)
        {
            return await _context.Funcionarios.FindAsync(id);
        }

        public async Task<bool> AdicionarAsync(FuncionarioViewModel model)
        {
            try
            {
                var funcionario = new Funcionario
                {
                    NomeCompleto = model.NomeCompleto,
               
                    CPF = model.CPF,
                    Endereco = model.Endereco,
                    Telefone = StringUtils.RemoverCaracteresEspeciais( model.Telefone),
                    Email = model.Email,
                    ContatoEmergencia = model.ContatoEmergencia,
                    TelefoneEmergencia = model.TelefoneEmergencia,
                    Cargo = model.Cargo,
                    NivelPermissao = model.NivelPermissao,
                    Turno = model.Turno,
                    Ativo = model.Ativo,
                    DataAdmissao = model.DataAdmissao,
                    DataCadastro = DateTime.Now
                };

                _context.Funcionarios.Add(funcionario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AtualizarAsync(int id, FuncionarioViewModel model)
        {
            try
            {
                var funcionario = await _context.Funcionarios.FindAsync(id);
                if (funcionario == null) return false;

                funcionario.NomeCompleto = model.NomeCompleto;
               
                funcionario.CPF = StringUtils.RemoverCaracteresEspeciais( model.CPF);
                funcionario.Endereco = model.Endereco;
                funcionario.Telefone = StringUtils.RemoverCaracteresEspeciais (model.Telefone);
                funcionario.Email = model.Email;
                funcionario.ContatoEmergencia = model.ContatoEmergencia;
                funcionario.TelefoneEmergencia = StringUtils.RemoverCaracteresEspeciais( model.TelefoneEmergencia);
                funcionario.Cargo = model.Cargo;
                funcionario.NivelPermissao = model.NivelPermissao;
                funcionario.Turno = model.Turno;
                funcionario.Ativo = model.Ativo;
                funcionario.DataAdmissao = model.DataAdmissao;

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
                var funcionario = await _context.Funcionarios.FindAsync(id);
                if (funcionario == null) return false;

                _context.Funcionarios.Remove(funcionario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CpfExisteAsync(string cpf, int? id = null)
        {
            return await _context.Funcionarios
                .AnyAsync(f => f.CPF == cpf && (!id.HasValue || f.Id != id.Value));
        }

        public async Task<List<string>> ObterCargosAsync()
        {
            return await _context.Funcionarios
                .Select(f => f.Cargo)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}