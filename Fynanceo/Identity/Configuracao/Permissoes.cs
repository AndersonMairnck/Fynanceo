namespace Fynanceo.Configuracao
{
    public static class Permissoes
    {
        // Clientes
        public const string ClientesVisualizar = "Clientes.Visualizar";
        public const string ClientesCriar = "Clientes.Criar";
        public const string ClientesEditar = "Clientes.Editar";
        public const string ClientesExcluir = "Clientes.Excluir";
        
        // Produtos
        public const string ProdutosVisualizar = "Produtos.Visualizar";
        public const string ProdutosCriar = "Produtos.Criar";
        public const string ProdutosEditar = "Produtos.Editar";
        public const string ProdutosExcluir = "Produtos.Excluir";
        
        // Financeiro
        public const string FinanceiroVisualizar = "Financeiro.Visualizar";
        public const string FinanceiroEditar = "Financeiro.Editar";
        public const string FinanceiroAprovar = "Financeiro.Aprovar";
        
        // Relatórios
        public const string RelatoriosVendas = "Relatorios.Vendas";
        public const string RelatoriosFinanceiro = "Relatorios.Financeiro";
        public const string RelatoriosEstoque = "Relatorios.Estoque";
        
        // Configurações
        public const string ConfiguracoesGerais = "Configuracoes.Gerais";
        public const string ConfiguracoesSistema = "Configuracoes.Sistema";

        // Método helper para obter todas as permissões
        public static Dictionary<string, List<string>> ObterTodasPorCategoria()
        {
            return new Dictionary<string, List<string>>
            {
                {
                    "Clientes", new List<string>
                    {
                        ClientesVisualizar,
                        ClientesCriar,
                        ClientesEditar,
                        ClientesExcluir
                    }
                },
                {
                    "Produtos", new List<string>
                    {
                        ProdutosVisualizar,
                        ProdutosCriar,
                        ProdutosEditar,
                        ProdutosExcluir
                    }
                },
                {
                    "Financeiro", new List<string>
                    {
                        FinanceiroVisualizar,
                        FinanceiroEditar,
                        FinanceiroAprovar
                    }
                },
                {
                    "Relatórios", new List<string>
                    {
                        RelatoriosVendas,
                        RelatoriosFinanceiro,
                        RelatoriosEstoque
                    }
                },
                {
                    "Configurações", new List<string>
                    {
                        ConfiguracoesGerais,
                        ConfiguracoesSistema
                    }
                }
            };
        }
    }
}