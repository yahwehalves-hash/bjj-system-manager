using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGatewayConfiguracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Colunas de gateway na configuração global
            migrationBuilder.AddColumn<string>(
                name: "gateway_tipo",
                table: "configuracao_global",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'Asaas'");

            migrationBuilder.AddColumn<bool>(
                name: "gerar_cobranca_online_automatico",
                table: "configuracao_global",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "lembrete_inadimplencia_ativo",
                table: "configuracao_global",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "dias_lembrete_apos_vencimento",
                table: "configuracao_global",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Tabela de catálogo de gateways disponíveis
            migrationBuilder.Sql("""
                CREATE TABLE gateways_disponiveis (
                    nome      varchar(20)  PRIMARY KEY,
                    descricao varchar(100) NOT NULL,
                    ativo     boolean      NOT NULL DEFAULT true
                );
                INSERT INTO gateways_disponiveis (nome, descricao, ativo) VALUES
                    ('Asaas',  'Asaas — PIX e Boleto',       true),
                    ('Nenhum', 'Sem gateway — baixa manual',  true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "gateway_tipo",                    table: "configuracao_global");
            migrationBuilder.DropColumn(name: "gerar_cobranca_online_automatico", table: "configuracao_global");
            migrationBuilder.DropColumn(name: "lembrete_inadimplencia_ativo",    table: "configuracao_global");
            migrationBuilder.DropColumn(name: "dias_lembrete_apos_vencimento",   table: "configuracao_global");
            migrationBuilder.Sql("DROP TABLE IF EXISTS gateways_disponiveis;");
        }
    }
}
