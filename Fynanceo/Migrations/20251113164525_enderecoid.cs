using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class enderecoid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnderecoEntregaId",
                table: "Entregas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 11, 13, 16, 45, 24, 429, DateTimeKind.Utc).AddTicks(6215));

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_EnderecoEntregaId",
                table: "Entregas",
                column: "EnderecoEntregaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entregas_EnderecosClientes_EnderecoEntregaId",
                table: "Entregas",
                column: "EnderecoEntregaId",
                principalTable: "EnderecosClientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entregas_EnderecosClientes_EnderecoEntregaId",
                table: "Entregas");

            migrationBuilder.DropIndex(
                name: "IX_Entregas_EnderecoEntregaId",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "EnderecoEntregaId",
                table: "Entregas");

            migrationBuilder.UpdateData(
                table: "CozinhaConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataAtualizacao",
                value: new DateTime(2025, 11, 3, 20, 5, 34, 403, DateTimeKind.Utc).AddTicks(5890));
        }
    }
}
