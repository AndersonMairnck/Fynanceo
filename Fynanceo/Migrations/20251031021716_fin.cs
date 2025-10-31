using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class fin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Caixas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SaldoInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalEntradas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalSaidas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaldoFisico = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UsuarioAberturaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioAberturaNome = table.Column<string>(type: "text", nullable: false),
                    UsuarioFechamentoId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioFechamentoNome = table.Column<string>(type: "text", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Fechado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caixas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Contato = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorPago = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FormaPagamento = table.Column<int>(type: "integer", nullable: true),
                    ParcelaAtual = table.Column<int>(type: "integer", nullable: true),
                    TotalParcelas = table.Column<int>(type: "integer", nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    PedidoId = table.Column<int>(type: "integer", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contas_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contas_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesCaixa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaixaId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FormaPagamento = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    PedidoId = table.Column<int>(type: "integer", nullable: true),
                    EntregaId = table.Column<int>(type: "integer", nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    DataMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioNome = table.Column<string>(type: "text", nullable: false),
                    IsSangria = table.Column<bool>(type: "boolean", nullable: true),
                    IsSuprimento = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesCaixa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesCaixa_Caixas_CaixaId",
                        column: x => x.CaixaId,
                        principalTable: "Caixas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesCaixa_Entregas_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entregas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimentacoesCaixa_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimentacoesCaixa_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MovimentacaoContas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContaId = table.Column<int>(type: "integer", nullable: false),
                    StatusAnterior = table.Column<int>(type: "integer", nullable: false),
                    StatusNovo = table.Column<int>(type: "integer", nullable: false),
                    ValorPago = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacao = table.Column<string>(type: "text", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioNome = table.Column<string>(type: "text", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacaoContas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacaoContas_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contas_FornecedorId",
                table: "Contas",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contas_PedidoId",
                table: "Contas",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacaoContas_ContaId",
                table: "MovimentacaoContas",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesCaixa_CaixaId",
                table: "MovimentacoesCaixa",
                column: "CaixaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesCaixa_EntregaId",
                table: "MovimentacoesCaixa",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesCaixa_FornecedorId",
                table: "MovimentacoesCaixa",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesCaixa_PedidoId",
                table: "MovimentacoesCaixa",
                column: "PedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacaoContas");

            migrationBuilder.DropTable(
                name: "MovimentacoesCaixa");

            migrationBuilder.DropTable(
                name: "Contas");

            migrationBuilder.DropTable(
                name: "Caixas");

            migrationBuilder.DropTable(
                name: "Fornecedores");
        }
    }
}
