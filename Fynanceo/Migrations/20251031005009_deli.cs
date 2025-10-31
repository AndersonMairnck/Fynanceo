using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class deli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesDelivery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaxaBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorMinimoGratis = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ComissaoBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RaioMaximoEntrega = table.Column<int>(type: "integer", nullable: false),
                    TempoEstimadoBase = table.Column<int>(type: "integer", nullable: false),
                    RegioesCobertas = table.Column<string>(type: "text", nullable: true),
                    HorariosFuncionamento = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesDelivery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entregadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: false),
                    TipoVeiculo = table.Column<int>(type: "integer", nullable: false),
                    Placa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ModeloVeiculo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CorVeiculo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    UltimaAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalEntregas = table.Column<int>(type: "integer", nullable: false),
                    AvaliacaoMedia = table.Column<decimal>(type: "numeric", nullable: false),
                    ComissaoTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entregas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    EntregadorId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EnderecoCompleto = table.Column<string>(type: "text", nullable: false),
                    Complemento = table.Column<string>(type: "text", nullable: true),
                    Referencia = table.Column<string>(type: "text", nullable: true),
                    Instrucoes = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataSaiuEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataPrevisao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxaEntrega = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ComissaoEntregador = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CodigoVerificacao = table.Column<string>(type: "text", nullable: true),
                    MotivoProblema = table.Column<string>(type: "text", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Avaliacao = table.Column<int>(type: "integer", nullable: true),
                    ComentarioAvaliacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entregas_Entregadores_EntregadorId",
                        column: x => x.EntregadorId,
                        principalTable: "Entregadores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Entregas_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoEntregas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntregaId = table.Column<int>(type: "integer", nullable: false),
                    StatusAnterior = table.Column<string>(type: "text", nullable: false),
                    StatusNovo = table.Column<string>(type: "text", nullable: false),
                    Observacao = table.Column<string>(type: "text", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioNome = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    DataAlteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoEntregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoEntregas_Entregas_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entregas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_EntregadorId",
                table: "Entregas",
                column: "EntregadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_PedidoId",
                table: "Entregas",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoEntregas_EntregaId",
                table: "HistoricoEntregas",
                column: "EntregaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesDelivery");

            migrationBuilder.DropTable(
                name: "HistoricoEntregas");

            migrationBuilder.DropTable(
                name: "Entregas");

            migrationBuilder.DropTable(
                name: "Entregadores");
        }
    }
}
