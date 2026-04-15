using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueIndexAtivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_atletas_cpf",
                table: "atletas");

            migrationBuilder.DropIndex(
                name: "IX_atletas_email",
                table: "atletas");

            migrationBuilder.CreateIndex(
                name: "IX_atletas_cpf",
                table: "atletas",
                column: "cpf",
                unique: true,
                filter: "ativo = true");

            migrationBuilder.CreateIndex(
                name: "IX_atletas_email",
                table: "atletas",
                column: "email",
                unique: true,
                filter: "ativo = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_atletas_cpf",
                table: "atletas");

            migrationBuilder.DropIndex(
                name: "IX_atletas_email",
                table: "atletas");

            migrationBuilder.CreateIndex(
                name: "IX_atletas_cpf",
                table: "atletas",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_atletas_email",
                table: "atletas",
                column: "email",
                unique: true);
        }
    }
}
