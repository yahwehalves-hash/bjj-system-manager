using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTurmas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "turmas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    professor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dias_semana = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    horario = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    capacidade_maxima = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turmas", x => x.id);
                    table.ForeignKey(
                        name: "FK_turmas_filiais_filial_id",
                        column: x => x.filial_id,
                        principalTable: "filiais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "atletas_turmas",
                columns: table => new
                {
                    atleta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    turma_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vinculado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atletas_turmas", x => new { x.atleta_id, x.turma_id });
                    table.ForeignKey(
                        name: "FK_atletas_turmas_atletas_atleta_id",
                        column: x => x.atleta_id,
                        principalTable: "atletas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_atletas_turmas_turmas_turma_id",
                        column: x => x.turma_id,
                        principalTable: "turmas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_atletas_turmas_turma_id",
                table: "atletas_turmas",
                column: "turma_id");

            migrationBuilder.CreateIndex(
                name: "IX_turmas_filial_id",
                table: "turmas",
                column: "filial_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atletas_turmas");

            migrationBuilder.DropTable(
                name: "turmas");
        }
    }
}
