using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class TareasUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioCreadorId",
                table: "Tareas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_UsuarioCreadorId",
                table: "Tareas",
                column: "UsuarioCreadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_AspNetUsers_UsuarioCreadorId",
                table: "Tareas",
                column: "UsuarioCreadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_AspNetUsers_UsuarioCreadorId",
                table: "Tareas");

            migrationBuilder.DropIndex(
                name: "IX_Tareas_UsuarioCreadorId",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "UsuarioCreadorId",
                table: "Tareas");
        }
    }
}
