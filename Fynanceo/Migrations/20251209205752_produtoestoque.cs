using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class produtoestoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "idEstoque",
                table: "Produtos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Revenda",
                table: "Estoques",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 12, 9, 20, 57, 49, 429, DateTimeKind.Utc).AddTicks(3144));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "idEstoque",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Revenda",
                table: "Estoques");

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 12, 8, 10, 13, 48, 300, DateTimeKind.Utc).AddTicks(9532));
        }
    }
}
