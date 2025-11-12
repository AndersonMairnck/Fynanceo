// ViewComponents/PedidosCardsViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Fynanceo.Models;

namespace Fynanceo.ViewComponents
{
    public class PedidosCardsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<Pedido> pedidos)
        {
            return View(pedidos);
        }
    }
}