using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class confg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateTable(
                name: "CozinhaConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TempoAlertaPreparoMinutos = table.Column<int>(type: "integer", nullable: false),
                    TempoAlertaProntoMinutos = table.Column<int>(type: "integer", nullable: false),
                    IntervaloAtualizacaoSegundos = table.Column<int>(type: "integer", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioAtualizacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CozinhaConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CozinhaConfigs",
                columns: new[] { "Id", "DataAtualizacao", "IntervaloAtualizacaoSegundos", "TempoAlertaPreparoMinutos", "TempoAlertaProntoMinutos", "UsuarioAtualizacao" },
                values: new object[] { 1, new DateTime(2025, 11, 3, 20, 5, 34, 403, DateTimeKind.Utc).AddTicks(5890), 30, 30, 10, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CozinhaConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
