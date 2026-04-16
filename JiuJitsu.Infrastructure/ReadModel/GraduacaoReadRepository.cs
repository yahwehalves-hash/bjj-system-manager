using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class GraduacaoReadRepository : IGraduacaoReadRepository
{
    private readonly string _connectionString;

    public GraduacaoReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<IEnumerable<RegraGraduacaoDto>> ListarRegrasAsync(Guid? filialId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                id          AS Id,
                filial_id   AS FilialId,
                faixa       AS Faixa,
                grau        AS Grau,
                tempo_minimo_meses AS TempoMinimoMeses
            FROM regras_graduacao
            WHERE filial_id = @FilialId OR (@FilialId IS NULL AND filial_id IS NULL)
            ORDER BY faixa, grau
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<RegraGraduacaoDto>(sql, new { FilialId = filialId });
    }

    public async Task<IEnumerable<ElegívelGraduacaoDto>> ListarElegiveisAsync(Guid? filialId, CancellationToken ct = default)
    {
        var filtroFilial = filialId.HasValue ? "AND a.filial_id = @FilialId" : string.Empty;

        var sql = $"""
            WITH regras AS (
                -- Usa regra específica da filial; se não existir, usa global
                SELECT DISTINCT ON (faixa, grau)
                    faixa, grau, tempo_minimo_meses, filial_id
                FROM regras_graduacao
                WHERE filial_id = @FilialId OR filial_id IS NULL
                ORDER BY faixa, grau, filial_id NULLS LAST
            )
            SELECT
                a.id                        AS AtletaId,
                a.nome_completo             AS NomeAtleta,
                a.filial_id                 AS FilialId,
                f.nome                      AS NomeFilial,
                a.faixa                     AS FaixaAtual,
                a.grau                      AS GrauAtual,
                a.data_ultima_graduacao     AS DataUltimaGraduacao,
                DATE_PART('month', AGE(CURRENT_DATE, a.data_ultima_graduacao))::int
                  + DATE_PART('year',  AGE(CURRENT_DATE, a.data_ultima_graduacao))::int * 12
                                            AS MesesNaFaixa,
                COALESCE(r.tempo_minimo_meses, 0) AS TempoMinimoNecessario
            FROM atletas a
            INNER JOIN filiais f ON f.id = a.filial_id
            LEFT JOIN regras r ON r.faixa = a.faixa AND r.grau = a.grau
            WHERE a.ativo = true {filtroFilial}
              AND (
                DATE_PART('month', AGE(CURRENT_DATE, a.data_ultima_graduacao))
                + DATE_PART('year', AGE(CURRENT_DATE, a.data_ultima_graduacao)) * 12
              ) >= COALESCE(r.tempo_minimo_meses, 0)
              AND r.tempo_minimo_meses IS NOT NULL
            ORDER BY a.faixa, a.grau, a.nome_completo
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<ElegívelGraduacaoDto>(sql, new { FilialId = filialId });
    }
}
