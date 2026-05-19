using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantenimiento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNombreContactoDependencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreContacto",
                table: "Dependencias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreContacto",
                table: "Dependencias");
        }
    }
}
