using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barberia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTokenPublicoTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TokenPublico",
                table: "Turnos",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_UsuarioId",
                table: "Turnos",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_Usuarios_UsuarioId",
                table: "Turnos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_Usuarios_UsuarioId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_UsuarioId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenPublico",
                table: "Turnos");
        }
    }
}
