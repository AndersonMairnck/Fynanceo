// Models/Fornecedor.cs
using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models
{
    public class Fornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(14)] // CPF/CNPJ
        public string? Documento { get; set; }

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

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        public StatusFornecedor Status { get; set; } = StatusFornecedor.Ativo;
        // Navigation Properties
        public ICollection<Conta> Contas { get; set; } = new List<Conta>();
        public ICollection<MovimentacaoCaixa> Movimentacoes { get; set; } = new List<MovimentacaoCaixa>();
    }

    // Models/Enums/StatusFornecedor.cs
  
}