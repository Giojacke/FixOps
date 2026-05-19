using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantenimiento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorreoQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorreosEncolados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Destinatario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Cuerpo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoCorreo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Intentos = table.Column<int>(type: "int", nullable: false),
                    MaxIntentos = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoIntento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProximoIntento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMensaje = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorreosEncolados", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorreosEncolados_Estado",
                table: "CorreosEncolados",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_CorreosEncolados_FechaCreacion",
                table: "CorreosEncolados",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_CorreosEncolados_ProximoIntento",
                table: "CorreosEncolados",
                column: "ProximoIntento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorreosEncolados");
        }
    }
}
