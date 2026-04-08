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
            SELECT
                f.id        AS FilialId,
                f.nome      AS NomeFilial,
                -- Indicadores de atletas
                COUNT(DISTINCT a.id) FILTER (WHERE a.ativo = true)                           AS TotalAtletasAtivos,
                COUNT(DISTINCT m_inad.atleta_id)                                              AS TotalInadimplentes,
                -- Mensalidades do mês
                COALESCE(SUM(m.valor)     FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia), 0) AS ReceitaPrevista,
                COALESCE(SUM(m.valor_pago) FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia AND m.status = 'Paga'), 0) AS ReceitaRealizada,
                COUNT(m.id) FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia AND m.status = 'Pendente')               AS MensalidadesPendentes,
                COUNT(m.id) FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia AND m.status IN ('Vencida','Inadimplente')) AS MensalidadesVencidas,
                -- Despesas do mês
                COALESCE(SUM(d.valor) FILTER (WHERE TO_CHAR(d.data_competencia, 'YYYY-MM') = @Competencia AND d.status != 'Cancelada'), 0) AS TotalDespesas
            FROM filiais f
            LEFT JOIN atletas a ON a.filial_id = f.id
            LEFT JOIN mensalidades m ON m.filial_id = f.id
            LEFT JOIN mensalidades m_inad ON m_inad.filial_id = f.id AND m_inad.status = 'Inadimplente'
            LEFT JOIN despesas d ON d.filial_id = f.id
            WHERE f.id = @FilialId
            GROUP BY f.id, f.nome
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstAsync<DashboardFilialDto>(sql, new { FilialId = filialId, Competencia = competencia });
    }

    public async Task<IEnumerable<DashboardFilialDto>> ObterDashboardConsolidadoAsync(
        string competencia,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                f.id        AS FilialId,
                f.nome      AS NomeFilial,
                COUNT(DISTINCT a.id) FILTER (WHERE a.ativo = true)                           AS TotalAtletasAtivos,
                COUNT(DISTINCT m_inad.atleta_id)                                              AS TotalInadimplentes,
                COALESCE(SUM(m.valor)     FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia), 0) AS ReceitaPrevista,
                COALESCE(SUM(m.valor_pago) FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia AND m.status = 'Paga'), 0) AS ReceitaRealizada,
                COUNT(m.id) FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia AND m.status = 'Pendente')               AS MensalidadesPendentes,
                COUNT(m.id) FILTER (WHERE TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia AND m.status IN ('Vencida','Inadimplente')) AS MensalidadesVencidas,
                COALESCE(SUM(d.valor) FILTER (WHERE TO_CHAR(d.data_competencia, 'YYYY-MM') = @Competencia AND d.status != 'Cancelada'), 0) AS TotalDespesas
            FROM filiais f
            LEFT JOIN atletas a ON a.filial_id = f.id
            LEFT JOIN mensalidades m ON m.filial_id = f.id
            LEFT JOIN mensalidades m_inad ON m_inad.filial_id = f.id AND m_inad.status = 'Inadimplente'
            LEFT JOIN despesas d ON d.filial_id = f.id
            WHERE f.ativo = true
            GROUP BY f.id, f.nome
            ORDER BY f.nome
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<DashboardFilialDto>(sql, new { Competencia = competencia });
    }
}
