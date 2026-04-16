using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class DashboardReadRepository : IDashboardReadRepository
{
    private readonly string _connectionString;

    public DashboardReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<DashboardFilialDto> ObterDashboardFilialAsync(
        Guid    filialId,
        string  competencia,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH atletas_agg AS (
                SELECT filial_id,
                    CAST(COUNT(*) AS int) AS total_ativos
                FROM atletas
                WHERE ativo = true
                GROUP BY filial_id
            ),
            inadimplentes_agg AS (
                SELECT filial_id,
                    CAST(COUNT(DISTINCT atleta_id) AS int) AS total_inadimplentes
                FROM mensalidades
                WHERE status = 'Inadimplente'
                GROUP BY filial_id
            ),
            mensalidades_agg AS (
                SELECT filial_id,
                    COALESCE(SUM(valor), 0)                                          AS receita_prevista,
                    COALESCE(SUM(valor_pago) FILTER (WHERE status = 'Paga'), 0)      AS receita_realizada,
                    CAST(COUNT(*) FILTER (WHERE status = 'Pendente')               AS int) AS mensalidades_pendentes,
                    CAST(COUNT(*) FILTER (WHERE status IN ('Vencida','Inadimplente')) AS int) AS mensalidades_vencidas
                FROM mensalidades
                WHERE TO_CHAR(competencia, 'YYYY-MM') = @Competencia
                GROUP BY filial_id
            ),
            despesas_agg AS (
                SELECT filial_id,
                    COALESCE(SUM(valor), 0) AS total_despesas
                FROM despesas
                WHERE TO_CHAR(data_competencia, 'YYYY-MM') = @Competencia
                  AND status != 'Cancelada'
                GROUP BY filial_id
            )
            SELECT
                f.id   AS FilialId,
                f.nome AS NomeFilial,
                COALESCE(aa.total_ativos,           0) AS TotalAtletasAtivos,
                COALESCE(ia.total_inadimplentes,    0) AS TotalInadimplentes,
                COALESCE(ma.receita_prevista,        0) AS ReceitaPrevista,
                COALESCE(ma.receita_realizada,       0) AS ReceitaRealizada,
                COALESCE(ma.mensalidades_pendentes,  0) AS MensalidadesPendentes,
                COALESCE(ma.mensalidades_vencidas,   0) AS MensalidadesVencidas,
                COALESCE(da.total_despesas,          0) AS TotalDespesas
            FROM filiais f
            LEFT JOIN atletas_agg      aa ON aa.filial_id = f.id
            LEFT JOIN inadimplentes_agg ia ON ia.filial_id = f.id
            LEFT JOIN mensalidades_agg  ma ON ma.filial_id = f.id
            LEFT JOIN despesas_agg      da ON da.filial_id = f.id
            WHERE f.id = @FilialId
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstAsync<DashboardFilialDto>(sql, new { FilialId = filialId, Competencia = competencia });
    }

    public async Task<IEnumerable<DashboardFilialDto>> ObterDashboardConsolidadoAsync(
        string competencia,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH atletas_agg AS (
                SELECT filial_id,
                    CAST(COUNT(*) AS int) AS total_ativos
                FROM atletas
                WHERE ativo = true
                GROUP BY filial_id
            ),
            inadimplentes_agg AS (
                SELECT filial_id,
                    CAST(COUNT(DISTINCT atleta_id) AS int) AS total_inadimplentes
                FROM mensalidades
                WHERE status = 'Inadimplente'
                GROUP BY filial_id
            ),
            mensalidades_agg AS (
                SELECT filial_id,
                    COALESCE(SUM(valor), 0)                                               AS receita_prevista,
                    COALESCE(SUM(valor_pago) FILTER (WHERE status = 'Paga'), 0)           AS receita_realizada,
                    CAST(COUNT(*) FILTER (WHERE status = 'Pendente')                AS int) AS mensalidades_pendentes,
                    CAST(COUNT(*) FILTER (WHERE status IN ('Vencida','Inadimplente')) AS int) AS mensalidades_vencidas
                FROM mensalidades
                WHERE TO_CHAR(competencia, 'YYYY-MM') = @Competencia
                GROUP BY filial_id
            ),
            despesas_agg AS (
                SELECT filial_id,
                    COALESCE(SUM(valor), 0) AS total_despesas
                FROM despesas
                WHERE TO_CHAR(data_competencia, 'YYYY-MM') = @Competencia
                  AND status != 'Cancelada'
                GROUP BY filial_id
            )
            SELECT
                f.id   AS FilialId,
                f.nome AS NomeFilial,
                COALESCE(aa.total_ativos,           0) AS TotalAtletasAtivos,
                COALESCE(ia.total_inadimplentes,    0) AS TotalInadimplentes,
                COALESCE(ma.receita_prevista,        0) AS ReceitaPrevista,
                COALESCE(ma.receita_realizada,       0) AS ReceitaRealizada,
                COALESCE(ma.mensalidades_pendentes,  0) AS MensalidadesPendentes,
                COALESCE(ma.mensalidades_vencidas,   0) AS MensalidadesVencidas,
                COALESCE(da.total_despesas,          0) AS TotalDespesas
            FROM filiais f
            LEFT JOIN atletas_agg       aa ON aa.filial_id = f.id
            LEFT JOIN inadimplentes_agg ia ON ia.filial_id = f.id
            LEFT JOIN mensalidades_agg  ma ON ma.filial_id = f.id
            LEFT JOIN despesas_agg      da ON da.filial_id = f.id
            WHERE f.ativo = true
            ORDER BY f.nome
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<DashboardFilialDto>(sql, new { Competencia = competencia });
    }
}
