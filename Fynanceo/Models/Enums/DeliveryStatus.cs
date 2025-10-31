// Models/Enums/DeliveryStatus.cs
namespace Fynanceo.Models.Enums
{
    public enum StatusEntregador
    {
        Disponivel = 1,
        Entregando = 2,
        Ausente = 3,
        Bloqueado = 4
    }

    public enum StatusEntrega
    {
        AguardandoEntregador = 1,
        SaiuParaEntrega = 2,
        EmRota = 3,
        Entregue = 4,
        Cancelada = 5,
        Problema = 6
    }

    public enum TipoVeiculo
    {
        Moto = 1,
        Carro = 2,
        Bicicleta = 3,
        Outro = 4
    }
}