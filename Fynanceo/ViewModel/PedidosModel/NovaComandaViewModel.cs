using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fynanceo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Fynanceo.Models.Enums;

namespace Fynanceo.ViewModel.PedidosModel
{
    public class NovaComandaViewModel
    {
        [Display(Name = "Tipo de Pedido")]
        [Required(ErrorMessage = "Tipo de pedido é obrigatório")]
        public TipoPedido TipoPedido { get; set; }

        [Display(Name = "Mesa")]
        public int? MesaId { get; set; }

        [Display(Name = "Cliente")]
        public int? ClienteId { get; set; }

        [Display(Name = "Garçom/Atendente")]
        [Required(ErrorMessage = "Garçom é obrigatório")]
        public int FuncionarioId { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500)]
        public string Observacoes { get; set; }

        [Display(Name = "Taxa de Entrega")]
        [DataType(DataType.Currency)]
        public decimal TaxaEntrega { get; set; }

        // Lists para dropdowns
        public List<Mesa> Mesas { get; set; }
        public List<Cliente> Clientes { get; set; }
        public List<Funcionario> Funcionarios { get; set; }

        public NovaComandaViewModel()
        {
            Mesas = new List<Mesa>();
            Clientes = new List<Cliente>();
            Funcionarios = new List<Funcionario>();
        }
    }

    public class PedidoDetalhesViewModel
    {
        public Pedido Pedido { get; set; }
        public List<Produto> Produtos { get; set; }
        public ItemPedido NovoItem { get; set; }

        public PedidoDetalhesViewModel()
        {
            Produtos = new List<Produto>();
        }
    }
}