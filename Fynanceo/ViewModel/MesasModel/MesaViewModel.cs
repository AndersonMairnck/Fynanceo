using System.ComponentModel.DataAnnotations;

namespace Fynanceo.ViewModel.MesasModel
{
    public class MesaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Número da mesa é obrigatório")]
        [Display(Name = "Número da Mesa")]
        public string Numero { get; set; }

        [Required(ErrorMessage = "Capacidade é obrigatória")]
        [Range(1, 50, ErrorMessage = "Capacidade deve ser entre 1 e 50 pessoas")]
        [Display(Name = "Capacidade (pessoas)")]
        public int Capacidade { get; set; }

        [Required(ErrorMessage = "Localização é obrigatória")]
        [Display(Name = "Localização")]
        public string Localizacao { get; set; }

        [Display(Name = "Ambiente")]
        public string Ambiente { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Livre";

        [Display(Name = "Descrição")]
        [StringLength(200, ErrorMessage = "Descrição não pode exceder 200 caracteres")]
        public string Descricao { get; set; }
    }
}