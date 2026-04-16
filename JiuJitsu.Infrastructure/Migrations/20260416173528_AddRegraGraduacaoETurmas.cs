using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegraGraduacaoETurmas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "regras_graduacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: true),
                    faixa = table.Column<string>(type: "text", nullable: false),
                    grau = table.Column<int>(type: "integer", nullable: false),
                    tempo_minimo_meses = table.Column<int>(type: "integer", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regras_graduacao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_regras_graduacao_filial_id_faixa_grau",
                table: "regras_graduacao",
                columns: new[] { "filial_id", "faixa", "grau" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "regras_graduacao");
        }
    }
}
