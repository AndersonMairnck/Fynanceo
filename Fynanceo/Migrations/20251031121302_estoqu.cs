using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class estoqu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstoqueId",
                table: "IngredientesProdutos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Fornecedores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Fornecedores",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Fornecedores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CategoriasEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estoques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EstoqueAtual = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    EstoqueMinimo = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    EstoqueMaximo = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnidadeMedida = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CategoriaEstoqueId = table.Column<int>(type: "integer", nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estoques_CategoriasEstoque_CategoriaEstoqueId",
                        column: x => x.CategoriaEstoqueId,
                        principalTable: "CategoriasEstoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estoques_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItensInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventarioId = table.Column<int>(type: "integer", nullable: false),
                    EstoqueId = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeSistema = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantidadeFisica = table.Column<decimal>(type: "numeric", nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Conferido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensInventario_Estoques_EstoqueId",
                        column: x => x.EstoqueId,
                        principalTable: "Estoques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensInventario_Inventarios_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstoqueId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CustoTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    PedidoId = table.Column<int>(type: "integer", nullable: true),
                    DataMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Estoques_EstoqueId",
                        column: x => x.EstoqueId,
                        principalTable: "Estoques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoIngredientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    EstoqueId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Observacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoIngredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutoIngredientes_Estoques_EstoqueId",
                        column: x => x.EstoqueId,
                        principalTable: "Estoques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProdutoIngredientes_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientesProdutos_EstoqueId",
                table: "IngredientesProdutos",
                column: "EstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_CategoriaEstoqueId",
                table: "Estoques",
                column: "CategoriaEstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_FornecedorId",
                table: "Estoques",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensInventario_EstoqueId",
                table: "ItensInventario",
                column: "EstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensInventario_InventarioId",
                table: "ItensInventario",
                column: "InventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_EstoqueId",
                table: "MovimentacoesEstoque",
                column: "EstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_FornecedorId",
                table: "MovimentacoesEstoque",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_PedidoId",
                table: "MovimentacoesEstoque",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoIngredientes_EstoqueId",
                table: "ProdutoIngredientes",
                column: "EstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoIngredientes_ProdutoId",
                table: "ProdutoIngredientes",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientesProdutos_Estoques_EstoqueId",
                table: "IngredientesProdutos",
                column: "EstoqueId",
                principalTable: "Estoques",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IngredientesProdutos_Estoques_EstoqueId",
                table: "IngredientesProdutos");

            migrationBuilder.DropTable(
                name: "ItensInventario");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "ProdutoIngredientes");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "Estoques");

            migrationBuilder.DropTable(
                name: "CategoriasEstoque");

            migrationBuilder.DropIndex(
                name: "IX_IngredientesProdutos_EstoqueId",
                table: "IngredientesProdutos");

            migrationBuilder.DropColumn(
                name: "EstoqueId",
                table: "IngredientesProdutos");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Fornecedores");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Fornecedores");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Fornecedores");
        }
    }
}
