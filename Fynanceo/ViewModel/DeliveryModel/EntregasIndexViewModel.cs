using Fynanceo.Models;

namespace Fynanceo.ViewModel.DeliveryModel;

public class EntregasIndexViewModel
{
    public IEnumerable<Entrega> Entregas { get; set; }
    public IEnumerable<Entregador> Entregadores { get; set; }
}