// Models/Enums/FinanceiroEnums.cs
namespace Fynanceo.Models.Enums
{
    public enum TipoMovimentacao
    {
        Entrada = 1,
        Saida = 2
    }

    public enum StatusConta
    {
        Pendente = 1,
        Paga = 2,
        Atrasada = 3,
        Cancelada = 4
    }

    public enum CategoriaFinanceira
    {
        Venda = 1,
        TaxaEntrega = 2,
        ComissaoEntregador = 3,
        Fornecedor = 4,
        Salario = 5,
        Aluguel = 6,
        Luz = 7,
        Agua = 8,
        Internet = 9,
        Manutencao = 10,
        Outros = 11
    }

    public enum FormaPagamento
    {
        Dinheiro = 1,
        CartaoCredito = 2,
        CartaoDebito = 3,
        Pix = 4,
        ValeRefeicao = 5,
        ValeAlimentacao = 6,
        Outros = 7
    }
}