using System.ComponentModel.DataAnnotations;
using Fynanceo.Models;
using Fynanceo.Utils;
namespace Fynanceo.ViewModel.FornecedorModel;

public class FornecerdorViewModel
{


    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; }
        
    [Display(Name = "Cpf/Cnpj")]
    [StringLength(18)] // CPF/CNPJ
    public string? CpfCnpj { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(100)]
    public string? Contato { get; set; }

    public string? Observacoes { get; set; }



    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }

    public StatusFornecedor Status { get; set; } = StatusFornecedor.Ativo;
    // // Navigation Properties
    // public ICollection<Conta> Contas { get; set; } = new List<Conta>();
    // public ICollection<MovimentacaoCaixa> Movimentacoes { get; set; } = new List<MovimentacaoCaixa>();
}

public class EditarFornecedorViewModel
{
    public int idFornecedor { get; set; }
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; }
        
    [Display(Name = "Cpf/Cnpj")]
    [StringLength(18)] // CPF/CNPJ
    public string? CpfCnpj { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(100)]
    public string? Contato { get; set; }

    public string? Observacoes { get; set; }

    public StatusFornecedor Status { get; set; } = StatusFornecedor.Ativo;
}

public class DetailsFornecedorViewModel
{
    public int idFornecedor { get; set; }
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; }
        
    [Display(Name = "Cpf/Cnpj")]
    [StringLength(18)] // CPF/CNPJ
    public string? CpfCnpj { get; set; }
   

    [StringLength(20)]
    public string? Telefone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(100)]
    public string? Contato { get; set; }

    public string? Observacoes { get; set; }



    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }

    public StatusFornecedor Status { get; set; } = StatusFornecedor.Ativo;
}