using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.ClientesModel
{
    public class EnderecoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Logradouro")]
        public string? Logradouro { get; set; }

        [Display(Name = "Número")]
        public string? Numero { get; set; }

        [Display(Name = "Complemento")]
        public string? Complemento { get; set; }

        [Display(Name = "Bairro")]
        public string? Bairro { get; set; }

        [Display(Name = "Cidade")]
        public string? Cidade { get; set; }

        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        [Display(Name = "CEP")]
        public string? Cep { get; set; }

        [Display(Name = "Referência")]
        public string? Referencia { get; set; }

        [Display(Name = "Principal")]
        public bool Principal { get; set; }
    }
}
