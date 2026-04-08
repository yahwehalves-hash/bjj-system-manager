using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinanceiroMultiFilial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "usuarios");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "usuarios",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "usuarios",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "usuarios",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "usuarios",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SenhaHash",
                table: "usuarios",
                newName: "senha_hash");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "usuarios",
                newName: "criado_em");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_Email",
                table: "usuarios",
                newName: "IX_usuarios_email");

            migrationBuilder.AddColumn<Guid>(
                name: "filial_id",
                table: "usuarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "filial_id",
                table: "atletas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios",
                column: "id");

            migrationBuilder.CreateTable(
                name: "configuracao_global",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_mensalidade_padrao = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    dia_vencimento = table.Column<int>(type: "integer", nullable: false),
                    tolerancia_inadimplencia_dias = table.Column<int>(type: "integer", nullable: false),
                    multa_atraso_percentual = table.Column<decimal>(type: "numeric(7,6)", nullable: false),
                    juros_diario_percentual = table.Column<decimal>(type: "numeric(7,6)", nullable: false),
                    desconto_antecipacao_percentual = table.Column<decimal>(type: "numeric(7,6)", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_por_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracao_global", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "filiais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    endereco = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filiais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracao_filial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_mensalidade_padrao = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    dia_vencimento = table.Column<int>(type: "integer", nullable: true),
                    tolerancia_inadimplencia_dias = table.Column<int>(type: "integer", nullable: true),
                    multa_atraso_percentual = table.Column<decimal>(type: "numeric(7,6)", nullable: true),
                    juros_diario_percentual = table.Column<decimal>(type: "numeric(7,6)", nullable: true),
                    desconto_antecipacao_percentual = table.Column<decimal>(type: "numeric(7,6)", nullable: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_por_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracao_filial", x => x.id);
                    table.ForeignKey(
                        name: "FK_configuracao_filial_filiais_filial_id",
                        column: x => x.filial_id,
                        principalTable: "filiais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "despesas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    subcategoria = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    data_competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    forma_pagamento = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    registrado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_despesas", x => x.id);
                    table.ForeignKey(
                        name: "FK_despesas_filiais_filial_id",
                        column: x => x.filial_id,
                        principalTable: "filiais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mensalidades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atleta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    forma_pagamento = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensalidades", x => x.id);
                    table.ForeignKey(
                        name: "FK_mensalidades_atletas_atleta_id",
                        column: x => x.atleta_id,
                        principalTable: "atletas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mensalidades_filiais_filial_id",
                        column: x => x.filial_id,
                        principalTable: "filiais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_filial_id",
                table: "usuarios",
                column: "filial_id");

            migrationBuilder.CreateIndex(
                name: "IX_atletas_filial_id",
                table: "atletas",
                column: "filial_id");

            migrationBuilder.CreateIndex(
                name: "IX_configuracao_filial_filial_id",
                table: "configuracao_filial",
                column: "filial_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_despesas_filial_id_data_competencia",
                table: "despesas",
                columns: new[] { "filial_id", "data_competencia" });

            migrationBuilder.CreateIndex(
                name: "IX_despesas_filial_id_status",
                table: "despesas",
                columns: new[] { "filial_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_filiais_cnpj",
                table: "filiais",
                column: "cnpj",
                unique: true,
                filter: "cnpj IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_mensalidades_atleta_id_competencia",
                table: "mensalidades",
                columns: new[] { "atleta_id", "competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mensalidades_data_vencimento_status",
                table: "mensalidades",
                columns: new[] { "data_vencimento", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_mensalidades_filial_id_status",
                table: "mensalidades",
                columns: new[] { "filial_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "FK_atletas_filiais_filial_id",
                table: "atletas",
                column: "filial_id",
                principalTable: "filiais",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_filiais_filial_id",
                table: "usuarios",
                column: "filial_id",
                principalTable: "filiais",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_atletas_filiais_filial_id",
                table: "atletas");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_filiais_filial_id",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "configuracao_filial");

            migrationBuilder.DropTable(
                name: "configuracao_global");

            migrationBuilder.DropTable(
                name: "despesas");

            migrationBuilder.DropTable(
                name: "mensalidades");

            migrationBuilder.DropTable(
                name: "filiais");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_filial_id",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_atletas_filial_id",
                table: "atletas");

            migrationBuilder.DropColumn(
                name: "filial_id",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "filial_id",
                table: "atletas");

            migrationBuilder.RenameTable(
                name: "usuarios",
                newName: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "Usuarios",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Usuarios",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Usuarios",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Usuarios",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "senha_hash",
                table: "Usuarios",
                newName: "SenhaHash");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "Usuarios",
                newName: "CriadoEm");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_email",
                table: "Usuarios",
                newName: "IX_Usuarios_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");
        }
    }
}
