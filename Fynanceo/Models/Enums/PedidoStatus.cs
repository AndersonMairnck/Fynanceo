// Models/Enums/PedidoStatus.cs
namespace Fynanceo.Models.Enums
{
    public enum PedidoStatus
    {
        Aberto = 1,
        EnviadoCozinha = 2,
        EmPreparo = 3,
        Pronto = 4,
        Entregue = 5,
        Fechado = 6,
        Cancelado = 7
    }

    public enum TipoPedido
    {
        Mesa = 1,
        Balcao = 2,
        Delivery = 3
    }

    public enum PrioridadePedido
    {
        Normal = 1,
        Urgente = 2,
        AguardandoConfirmacao = 3
    }
}