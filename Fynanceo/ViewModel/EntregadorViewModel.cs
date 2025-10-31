// ViewModels/EntregadorViewModel.cs
using System.ComponentModel.DataAnnotations;
using Fynanceo.Models.Enums;

namespace Fynanceo.ViewModels
{
    public class EntregadorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "Tipo de veículo é obrigatório")]
        [Display(Name = "Tipo de Veículo")]
        public TipoVeiculo TipoVeiculo { get; set; }

        [Display(Name = "Placa do Veículo")]
        public string? Placa { get; set; }

        [Display(Name = "Modelo do Veículo")]
        public string? ModeloVeiculo { get; set; }

        [Display(Name = "Cor do Veículo")]
        public string? CorVeiculo { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;
    }
}