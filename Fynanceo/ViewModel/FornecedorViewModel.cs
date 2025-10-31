// ViewModels/FornecedorViewModel.cs
using Fynanceo.Models;
using System.ComponentModel.DataAnnotations;

public class FornecedorViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do fornecedor é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter até 100 caracteres")]
    public string Nome { get; set; }

    [StringLength(14)]
    [Display(Name = "CNPJ/CPF")]
    public string Documento { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(20)]
    public string Telefone { get; set; }

    [Required]
    public StatusFornecedor Status { get; set; }

    [StringLength(200)]
    public string Endereco { get; set; }

    [StringLength(100)]
    public string Cidade { get; set; }

    [StringLength(2)]
    public string Estado { get; set; }

    [StringLength(9)]
    public string Cep { get; set; }

    [StringLength(100)]
    public string Contato { get; set; }

    [StringLength(500)]
    public string Observacao { get; set; }
}