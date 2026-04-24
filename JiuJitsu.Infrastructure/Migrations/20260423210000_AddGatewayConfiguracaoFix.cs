using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiuJitsu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGatewayConfiguracaoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Garante que as colunas existam (a migration anterior foi registrada vazia)
            migrationBuilder.Sql("""
                ALTER TABLE configuracao_global
                    ADD COLUMN IF NOT EXISTS gateway_tipo                     varchar(20)  NOT NULL DEFAULT 'Asaas',
                    ADD COLUMN IF NOT EXISTS gerar_cobranca_online_automatico boolean      NOT NULL DEFAULT true,
                    ADD COLUMN IF NOT EXISTS lembrete_inadimplencia_ativo     boolean      NOT NULL DEFAULT true,
                    ADD COLUMN IF NOT EXISTS dias_lembrete_apos_vencimento    integer      NOT NULL DEFAULT 1;

                CREATE TABLE IF NOT EXISTS gateways_disponiveis (
                    nome      varchar(20)  PRIMARY KEY,
                    descricao varchar(100) NOT NULL,
                    ativo     boolean      NOT NULL DEFAULT true
                );

                INSERT INTO gateways_disponiveis (nome, descricao, ativo) VALUES
                    ('Asaas',  'Asaas — PIX e Boleto',      true),
                    ('Nenhum', 'Sem gateway — baixa manual', true)
                ON CONFLICT (nome) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE configuracao_global
                    DROP COLUMN IF EXISTS gateway_tipo,
                    DROP COLUMN IF EXISTS gerar_cobranca_online_automatico,
                    DROP COLUMN IF EXISTS lembrete_inadimplencia_ativo,
                    DROP COLUMN IF EXISTS dias_lembrete_apos_vencimento;

                DROP TABLE IF EXISTS gateways_disponiveis;
                """);
        }
    }
}
