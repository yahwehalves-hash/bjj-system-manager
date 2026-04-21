using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPresenca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "registros_presenca",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atleta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    turma_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: false),
                    registrado_por = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_presenca", x => x.id);
                    table.ForeignKey(
                        name: "FK_registros_presenca_atletas_atleta_id",
                        column: x => x.atleta_id,
                        principalTable: "atletas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registros_presenca_turmas_turma_id",
                        column: x => x.turma_id,
                        principalTable: "turmas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registros_presenca_atleta_id",
                table: "registros_presenca",
                column: "atleta_id");

            migrationBuilder.CreateIndex(
                name: "IX_registros_presenca_atleta_id_turma_id_data_hora",
                table: "registros_presenca",
                columns: new[] { "atleta_id", "turma_id", "data_hora" });

            migrationBuilder.CreateIndex(
                name: "IX_registros_presenca_filial_id_data_hora",
                table: "registros_presenca",
                columns: new[] { "filial_id", "data_hora" });

            migrationBuilder.CreateIndex(
                name: "IX_registros_presenca_turma_id",
                table: "registros_presenca",
                column: "turma_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registros_presenca");
        }
    }
}
