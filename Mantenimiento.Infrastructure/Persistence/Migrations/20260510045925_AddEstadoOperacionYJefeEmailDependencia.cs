using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantenimiento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoOperacionYJefeEmailDependencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Operaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "JefeEmail",
                table: "Dependencias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "JefeEmail",
                table: "Dependencias");
        }
    }
}
