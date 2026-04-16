using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContratos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contratos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atleta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hash_documento = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ip_aceite = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    data_aceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pdf_bytes = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contratos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "templates_contrato",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conteudo = table.Column<string>(type: "text", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates_contrato", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contratos_atleta_id",
                table: "contratos",
                column: "atleta_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contratos");

            migrationBuilder.DropTable(
                name: "templates_contrato");
        }
    }
}
