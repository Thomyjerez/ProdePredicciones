using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProdePrediccionesAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaPredicciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Predicciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    PartidoId = table.Column<int>(type: "integer", nullable: false),
                    GolesLocalPredichos = table.Column<int>(type: "integer", nullable: false),
                    GolesVisitantePredichos = table.Column<int>(type: "integer", nullable: false),
                    PuntosObtenidos = table.Column<int>(type: "integer", nullable: false),
                    FechaPredicion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predicciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predicciones_Partidos_PartidoId",
                        column: x => x.PartidoId,
                        principalTable: "Partidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predicciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Predicciones_PartidoId",
                table: "Predicciones",
                column: "PartidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Predicciones_UsuarioId",
                table: "Predicciones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Predicciones");
        }
    }
}
