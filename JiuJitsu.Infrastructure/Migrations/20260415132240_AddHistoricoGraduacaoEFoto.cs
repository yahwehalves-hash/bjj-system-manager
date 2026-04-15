using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricoGraduacaoEFoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "foto_base64",
                table: "atletas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "historico_graduacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atleta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    faixa = table.Column<string>(type: "text", nullable: false),
                    grau = table.Column<int>(type: "integer", nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historico_graduacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_historico_graduacoes_atletas_atleta_id",
                        column: x => x.atleta_id,
                        principalTable: "atletas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_historico_graduacoes_atleta_id_data_inicio",
                table: "historico_graduacoes",
                columns: new[] { "atleta_id", "data_inicio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historico_graduacoes");

            migrationBuilder.DropColumn(
                name: "foto_base64",
                table: "atletas");
        }
    }
}
