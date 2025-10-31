// Models/Entregador.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fynanceo.Models.Enums;

namespace Fynanceo.Models
{
    public class Entregador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "Tipo de veículo é obrigatório")]
        public TipoVeiculo TipoVeiculo { get; set; }

        [StringLength(20)]
        public string? Placa { get; set; }

        [StringLength(50)]
        public string? ModeloVeiculo { get; set; }

        [StringLength(20)]
        public string? CorVeiculo { get; set; }

        [Required]
        public StatusEntregador Status { get; set; } = StatusEntregador.Disponivel;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime? UltimaAtualizacao { get; set; }

        // Estatísticas
        public int TotalEntregas { get; set; } = 0;
        public decimal AvaliacaoMedia { get; set; } = 5.0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ComissaoTotal { get; set; } = 0;

        public string? Observacoes { get; set; }

        public bool Ativo { get; set; } = true;

        // Navigation Properties
        public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
    }
}