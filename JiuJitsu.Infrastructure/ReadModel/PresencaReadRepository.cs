using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class PresencaReadRepository : IPresencaReadRepository
{
    private readonly string _connectionString;

    public PresencaReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<ListaPresencasDto> ListarPorTurmaAsync(
        Guid turmaId, DateOnly dataInicio, DateOnly dataFim,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                rp.id           AS Id,
                rp.atleta_id    AS AtletaId,
                a.nome_completo AS NomeAtleta,
                rp.turma_id     AS TurmaId,
                t.nome          AS NomeTurma,
                rp.filial_id    AS FilialId,
                rp.data_hora    AS DataHora,
                rp.origem       AS Origem
            FROM registros_presenca rp
            INNER JOIN atletas a ON a.id = rp.atleta_id
            INNER JOIN turmas  t ON t.id = rp.turma_id
            WHERE rp.turma_id = @TurmaId
              AND rp.data_hora::date BETWEEN @DataInicio AND @DataFim
            ORDER BY rp.data_hora DESC, a.nome_completo
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        var itens = (await conexao.QueryAsync<RegistroPresencaDto>(sql, new
        {
            TurmaId    = turmaId,
            DataInicio = dataInicio.ToDateTime(TimeOnly.MinValue),
            DataFim    = dataFim.ToDateTime(TimeOnly.MaxValue)
        })).ToList();

        return new ListaPresencasDto(itens, itens.Count);
    }

    public async Task<IEnumerable<FrequenciaAtletaDto>> FrequenciaPorTurmaAsync(
        Guid turmaId, DateOnly dataInicio, DateOnly dataFim,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH atletas_turma AS (
                SELECT a.id, a.nome_completo
                FROM atletas_turmas at2
                INNER JOIN atletas a ON a.id = at2.atleta_id
                WHERE at2.turma_id = @TurmaId
            ),
            presencas AS (
                SELECT atleta_id, COUNT(*) AS total_presencas
                FROM registros_presenca
                WHERE turma_id = @TurmaId
                  AND data_hora::date BETWEEN @DataInicio AND @DataFim
                GROUP BY atleta_id
            ),
            ultima AS (
                SELECT atleta_id, MAX(data_hora) AS ultima_presenca
                FROM registros_presenca
                WHERE turma_id = @TurmaId
                GROUP BY atleta_id
            ),
            total_aulas AS (
                SELECT COUNT(DISTINCT data_hora::date) AS total
                FROM registros_presenca
                WHERE turma_id = @TurmaId
                  AND data_hora::date BETWEEN @DataInicio AND @DataFim
            )
            SELECT
                at2.id                                              AS AtletaId,
                at2.nome_completo                                   AS NomeAtleta,
                @TurmaId::uuid                                      AS TurmaId,
                t.nome                                              AS NomeTurma,
                COALESCE(p.total_presencas, 0)::int                 AS TotalPresencas,
                ta.total::int                                       AS TotalAulas,
                CASE WHEN ta.total > 0
                    THEN ROUND(COALESCE(p.total_presencas, 0)::numeric / ta.total * 100, 1)
                    ELSE 0
                END                                                 AS PercentualFrequencia,
                u.ultima_presenca                                   AS UltimaPresenca
            FROM atletas_turma at2
            CROSS JOIN total_aulas ta
            INNER JOIN turmas t ON t.id = @TurmaId
            LEFT JOIN presencas p ON p.atleta_id = at2.id
            LEFT JOIN ultima    u ON u.atleta_id = at2.id
            ORDER BY at2.nome_completo
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<FrequenciaAtletaDto>(sql, new
        {
            TurmaId    = turmaId,
            DataInicio = dataInicio.ToDateTime(TimeOnly.MinValue),
            DataFim    = dataFim.ToDateTime(TimeOnly.MaxValue)
        });
    }

    public async Task<IEnumerable<FrequenciaAtletaDto>> FrequenciaPorAtletaAsync(
        Guid atletaId, DateOnly dataInicio, DateOnly dataFim,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH presencas AS (
                SELECT turma_id, COUNT(*) AS total_presencas, MAX(data_hora) AS ultima_presenca
                FROM registros_presenca
                WHERE atleta_id = @AtletaId
                  AND data_hora::date BETWEEN @DataInicio AND @DataFim
                GROUP BY turma_id
            ),
            total_aulas AS (
                SELECT turma_id, COUNT(DISTINCT data_hora::date) AS total
                FROM registros_presenca
                WHERE turma_id IN (SELECT turma_id FROM presencas)
                  AND data_hora::date BETWEEN @DataInicio AND @DataFim
                GROUP BY turma_id
            )
            SELECT
                @AtletaId::uuid                                      AS AtletaId,
                a.nome_completo                                      AS NomeAtleta,
                p.turma_id                                           AS TurmaId,
                t.nome                                               AS NomeTurma,
                p.total_presencas::int                               AS TotalPresencas,
                ta.total::int                                        AS TotalAulas,
                ROUND(p.total_presencas::numeric / ta.total * 100, 1) AS PercentualFrequencia,
                p.ultima_presenca                                    AS UltimaPresenca
            FROM presencas p
            INNER JOIN total_aulas ta ON ta.turma_id = p.turma_id
            INNER JOIN turmas t       ON t.id = p.turma_id
            INNER JOIN atletas a      ON a.id = @AtletaId
            ORDER BY t.nome
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<FrequenciaAtletaDto>(sql, new
        {
            AtletaId   = atletaId,
            DataInicio = dataInicio.ToDateTime(TimeOnly.MinValue),
            DataFim    = dataFim.ToDateTime(TimeOnly.MaxValue)
        });
    }

    public async Task<IEnumerable<AtletaInativoDto>> ListarInativosAsync(
        Guid? filialId, int diasSemPresenca,
        CancellationToken cancellationToken = default)
    {
        var filtroFilial = filialId.HasValue ? "AND a.filial_id = @FilialId" : string.Empty;

        var sql = $"""
            WITH ultima_presenca AS (
                SELECT atleta_id, MAX(data_hora) AS ultima
                FROM registros_presenca
                GROUP BY atleta_id
            )
            SELECT
                a.id                                                AS AtletaId,
                a.nome_completo                                     AS NomeAtleta,
                a.telefone                                          AS Telefone,
                a.email                                             AS Email,
                a.filial_id                                         AS FilialId,
                f.nome                                              AS NomeFilial,
                up.ultima                                           AS UltimaPresenca,
                EXTRACT(DAY FROM NOW() - up.ultima)::int            AS DiasInativo
            FROM atletas a
            INNER JOIN filiais f ON f.id = a.filial_id
            INNER JOIN ultima_presenca up ON up.atleta_id = a.id
            WHERE a.ativo = true
              {filtroFilial}
              AND EXTRACT(DAY FROM NOW() - up.ultima) >= @DiasInativo
            ORDER BY DiasInativo DESC, a.nome_completo
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<AtletaInativoDto>(sql, new
        {
            FilialId     = filialId,
            DiasInativo  = diasSemPresenca
        });
    }
}
