using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "atletas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_completo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    faixa = table.Column<string>(type: "text", nullable: false),
                    grau = table.Column<int>(type: "integer", nullable: false),
                    data_ultima_graduacao = table.Column<DateOnly>(type: "date", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atletas", x => x.id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atletas");
        }
    }
}
