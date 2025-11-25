// Controllers/FornecedoresController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fynanceo.Data;
using Fynanceo.Models;
using Fynanceo.Service.Interface;
using Fynanceo.ViewModel.FornecedorModel;
using Fynanceo.Utils;

namespace Fynanceo.Controllers
{
    public class FornecedoresController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFornecedorService _fornecedorService;

        public FornecedoresController(AppDbContext context, IFornecedorService fornecedorService)
        {
            _context = context;
            _fornecedorService = fornecedorService;
        }

        // GET: Fornecedores
        public async Task<IActionResult> Index()
        {
            var fornecedores = await _fornecedorService.ObterTodosFornecedoresAsync();
           
            return View(fornecedores);
        }

        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var fornecedor = await _fornecedorService.obterFornecedorPorId(id);
               

            if (fornecedor == null)
            {
                return NotFound();
            }
            // Monta o ViewModel que a View espera
            var viewModel = new DetailsFornecedorViewModel
            {
                idFornecedor = fornecedor.Id,
                Nome = fornecedor.Nome,
                CpfCnpj = StringUtils.FormatarCpfCnpj(fornecedor.CpfCnpj),
                Telefone = StringUtils.FormataTelefone(fornecedor.Telefone),
                Email = fornecedor.Email,
                Endereco = fornecedor.Endereco,
                Contato = fornecedor.Contato,
                Observacoes = fornecedor.Observacoes,
               
                DataCriacao = fornecedor.DataCriacao,
                DataAtualizacao = fornecedor.DataAtualizacao,
                Status = fornecedor.Status
            };

            return View(viewModel);
        }

        // GET: Fornecedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fornecedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FornecerdorViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
               await _fornecedorService.AdicionarAsync(viewModel);
              

                TempData["Success"] = "Fornecedor cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int id)
        {

            var fornecedor = await _fornecedorService.obterFornecedorPorId(id);

            if (fornecedor == null)
                return NotFound();

            var vm = new EditarFornecedorViewModel
            {
                idFornecedor = fornecedor.Id,
                Nome = fornecedor.Nome,
                CpfCnpj = StringUtils.FormatarCpfCnpj( fornecedor.CpfCnpj),
                Telefone = StringUtils.FormataTelefone( fornecedor.Telefone),
                Email = fornecedor.Email,
                Endereco = fornecedor.Endereco,
                Contato = fornecedor.Contato,
                Observacoes = fornecedor.Observacoes,
                Status = fornecedor.Status
            };

            return View(vm);
        }

        // POST: Fornecedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarFornecedorViewModel viewModel)
        {
            

            if (ModelState.IsValid)
            {
                try
                {
                 await   _fornecedorService.EditarFornecedor(id, viewModel);
                  

                    TempData["Success"] = "Fornecedor atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FornecedorExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        

        private bool FornecedorExists(int id)
        {
            return _context.Fornecedores.Any(e => e.Id == id);
        }
    }
}