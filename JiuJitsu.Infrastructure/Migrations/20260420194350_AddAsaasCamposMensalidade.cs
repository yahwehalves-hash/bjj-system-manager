using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAsaasCamposMensalidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "asaas_cobranca_id",
                table: "mensalidades",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "link_pagamento",
                table: "mensalidades",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pix_copia_cola",
                table: "mensalidades",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "asaas_cobranca_id",
                table: "mensalidades");

            migrationBuilder.DropColumn(
                name: "link_pagamento",
                table: "mensalidades");

            migrationBuilder.DropColumn(
                name: "pix_copia_cola",
                table: "mensalidades");
        }
    }
}
