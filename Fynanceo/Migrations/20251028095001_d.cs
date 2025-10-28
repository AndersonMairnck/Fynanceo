using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fynanceo.Migrations
{
    /// <inheritdoc />
    public partial class d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RG",
                table: "Funcionarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RG",
                table: "Funcionarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
