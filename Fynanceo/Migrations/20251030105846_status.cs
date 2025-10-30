using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmPreparo",
                table: "ItensPedido");

            migrationBuilder.DropColumn(
                name: "EnviadoCozinha",
                table: "ItensPedido");

            migrationBuilder.DropColumn(
                name: "Pronto",
                table: "ItensPedido");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ItensPedido",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ItensPedido");

            migrationBuilder.AddColumn<bool>(
                name: "EmPreparo",
                table: "ItensPedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnviadoCozinha",
                table: "ItensPedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pronto",
                table: "ItensPedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
