namespace Fynanceo.Configuracao
{
    public static class Politicas
    {
        // Políticas por área
        public const string AcessoAdministrativo = "AcessoAdministrativo";
        public const string AcessoGerencial = "AcessoGerencial";
        public const string AcessoCaixa = "AcessoCaixa";
        public const string AcessoCozinha = "AcessoCozinha";
        public const string AcessoDelivery = "AcessoDelivery";
        public const string AcessoAtendimento = "AcessoAtendimento";

        // Políticas por funcionalidade
        public const string GerenciarUsuarios = "GerenciarUsuarios";
        public const string GerenciarProdutos = "GerenciarProdutos";
        public const string GerenciarEstoque = "GerenciarEstoque";
        public const string GerenciarFinanceiro = "GerenciarFinanceiro";
        public const string VisualizarRelatorios = "VisualizarRelatorios";
        public const string RealizarVendas = "RealizarVendas";
    }
}