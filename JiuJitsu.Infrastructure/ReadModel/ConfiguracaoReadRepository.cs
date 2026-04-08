using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class ConfiguracaoReadRepository : IConfiguracaoReadRepository
{
    private readonly string _connectionString;

    public ConfiguracaoReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<ConfiguracaoGlobalDto?> ObterGlobalAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id,
                valor_mensalidade_padrao        AS ValorMensalidadePadrao,
                dia_vencimento                  AS DiaVencimento,
                tolerancia_inadimplencia_dias   AS ToleranciaInadimplenciaDias,
                multa_atraso_percentual         AS MultaAtrasoPercentual,
                juros_diario_percentual         AS JurosDiarioPercentual,
                desconto_antecipacao_percentual AS DescontoAntecipacaoPercentual,
                atualizado_em                   AS AtualizadoEm
            FROM configuracao_global
            LIMIT 1
            """;
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstOrDefaultAsync<ConfiguracaoGlobalDto>(sql);
    }

    public async Task<ConfiguracaoFilialDto?> ObterPorFilialAsync(Guid filialId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id,
                filial_id                       AS FilialId,
                valor_mensalidade_padrao        AS ValorMensalidadePadrao,
                dia_vencimento                  AS DiaVencimento,
                tolerancia_inadimplencia_dias   AS ToleranciaInadimplenciaDias,
                multa_atraso_percentual         AS MultaAtrasoPercentual,
                juros_diario_percentual         AS JurosDiarioPercentual,
                desconto_antecipacao_percentual AS DescontoAntecipacaoPercentual,
                atualizado_em                   AS AtualizadoEm
            FROM configuracao_filial
            WHERE filial_id = @FilialId
            """;
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstOrDefaultAsync<ConfiguracaoFilialDto>(sql, new { FilialId = filialId });
    }

    public async Task<ConfiguracaoEfetivaDto> ObterEfetivaAsync(Guid filialId, CancellationToken cancellationToken = default)
    {
        // Retorna a configuração efetiva: valor da filial se existir, senão o global
        const string sql = """
            SELECT
                COALESCE(cf.valor_mensalidade_padrao,        cg.valor_mensalidade_padrao)        AS ValorMensalidadePadrao,
                COALESCE(cf.dia_vencimento,                  cg.dia_vencimento)                  AS DiaVencimento,
                COALESCE(cf.tolerancia_inadimplencia_dias,   cg.tolerancia_inadimplencia_dias)   AS ToleranciaInadimplenciaDias,
                COALESCE(cf.multa_atraso_percentual,         cg.multa_atraso_percentual)         AS MultaAtrasoPercentual,
                COALESCE(cf.juros_diario_percentual,         cg.juros_diario_percentual)         AS JurosDiarioPercentual,
                COALESCE(cf.desconto_antecipacao_percentual, cg.desconto_antecipacao_percentual) AS DescontoAntecipacaoPercentual
            FROM configuracao_global cg
            LEFT JOIN configuracao_filial cf ON cf.filial_id = @FilialId
            LIMIT 1
            """;
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstAsync<ConfiguracaoEfetivaDto>(sql, new { FilialId = filialId });
    }
}
