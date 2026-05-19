using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantenimiento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RegistrarActividad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetalleFinalizacion",
                table: "Operaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "Operaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicio",
                table: "Operaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MotivoPausa",
                table: "Operaciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SolicitudesMateriales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreMaterial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    EsPersonalizado = table.Column<bool>(type: "bit", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesMateriales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesMateriales_Materiales_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitudesMateriales_Operaciones_OperacionId",
                        column: x => x.OperacionId,
                        principalTable: "Operaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMateriales_MaterialId",
                table: "SolicitudesMateriales",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMateriales_OperacionId",
                table: "SolicitudesMateriales",
                column: "OperacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesMateriales");

            migrationBuilder.DropColumn(
                name: "DetalleFinalizacion",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "Operaciones");

            migrationBuilder.DropColumn(
                name: "MotivoPausa",
                table: "Operaciones");
        }
    }
}
