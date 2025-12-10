using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class tipoitem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Revenda",
                table: "Estoques");

            migrationBuilder.AddColumn<string>(
                name: "TipoItem",
                table: "Estoques",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 12, 9, 21, 16, 17, 894, DateTimeKind.Utc).AddTicks(9202));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoItem",
                table: "Estoques");

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
    }
}
