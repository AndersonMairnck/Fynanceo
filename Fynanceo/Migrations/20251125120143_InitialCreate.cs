using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Documento",
                table: "Fornecedores",
                newName: "CpfCnpj");

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 11, 25, 12, 1, 37, 375, DateTimeKind.Utc).AddTicks(1833));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CpfCnpj",
                table: "Fornecedores",
                newName: "Documento");

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 11, 24, 22, 19, 37, 589, DateTimeKind.Utc).AddTicks(6338));
        }
    }
}
