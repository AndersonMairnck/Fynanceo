using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class pr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aliquota",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "CST",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "CodigoNCM",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "MotivoIndisponibilidade",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Origem",
                table: "Produtos");

            migrationBuilder.AlterColumn<string>(
                name: "JustificativaStatus",
                table: "Clientes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Aliquota",
                table: "Produtos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CST",
                table: "Produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodigoNCM",
                table: "Produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotivoIndisponibilidade",
                table: "Produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Origem",
                table: "Produtos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "JustificativaStatus",
                table: "Clientes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
