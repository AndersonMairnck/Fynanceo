using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class ie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoItem",
                table: "Estoques",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 12, 9, 22, 8, 33, 240, DateTimeKind.Utc).AddTicks(4747));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoItem",
                table: "Estoques",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1)",
                oldMaxLength: 1);

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 12, 9, 21, 16, 17, 894, DateTimeKind.Utc).AddTicks(9202));
        }
    }
}
