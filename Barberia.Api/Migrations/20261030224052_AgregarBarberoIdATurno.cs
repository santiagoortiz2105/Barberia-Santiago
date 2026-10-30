using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barberia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBarberoIdATurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BarberoId",
                table: "Turnos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_BarberoId",
                table: "Turnos",
                column: "BarberoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_Usuarios_BarberoId",
                table: "Turnos",
                column: "BarberoId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_Usuarios_BarberoId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_BarberoId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "BarberoId",
                table: "Turnos");
        }
    }
}
