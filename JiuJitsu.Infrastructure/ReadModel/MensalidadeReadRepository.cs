using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class MensalidadeReadRepository : IMensalidadeReadRepository
{
    private readonly string _connectionString;

    public MensalidadeReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<MensalidadeDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                m.id,
                m.atleta_id             AS AtletaId,
                a.nome_completo         AS NomeAtleta,
                m.filial_id             AS FilialId,
                f.nome                  AS NomeFilial,
                m.competencia,
                m.valor,
                m.valor_pago            AS ValorPago,
                m.data_vencimento       AS DataVencimento,
                m.data_pagamento        AS DataPagamento,
                m.status,
                m.forma_pagamento       AS FormaPagamento,
                m.observacao,
                m.criado_em             AS CriadoEm,
                m.atualizado_em         AS AtualizadoEm
            FROM mensalidades m
            INNER JOIN atletas a ON a.id = m.atleta_id
            INNER JOIN filiais f ON f.id = m.filial_id
            WHERE m.id = @Id
            """;
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstOrDefaultAsync<MensalidadeDetalheDto>(sql, new { Id = id });
    }

    public async Task<ListaMensalidadesDto> ListarAsync(
        Guid?   filialId,
        Guid?   atletaId,
        string? status,
        string? competencia,
        int     pagina,
        int     tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        var condicoes = new List<string>();
        if (filialId.HasValue)                    condicoes.Add("m.filial_id = @FilialId");
        if (atletaId.HasValue)                    condicoes.Add("m.atleta_id = @AtletaId");
        if (!string.IsNullOrWhiteSpace(status))   condicoes.Add("m.status = @Status");
        if (!string.IsNullOrWhiteSpace(competencia)) condicoes.Add("TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia");

        var where = condicoes.Count > 0 ? "WHERE " + string.Join(" AND ", condicoes) : string.Empty;

        var sqlContagem = $"""
            SELECT COUNT(*) FROM mensalidades m {where}
            """;

        var sqlItens = $"""
            SELECT
                m.id,
                m.atleta_id         AS AtletaId,
                a.nome_completo     AS NomeAtleta,
                m.filial_id         AS FilialId,
                f.nome              AS NomeFilial,
                m.competencia,
                m.valor,
                m.valor_pago        AS ValorPago,
                m.data_vencimento   AS DataVencimento,
                m.data_pagamento    AS DataPagamento,
                m.status,
                m.forma_pagamento   AS FormaPagamento
            FROM mensalidades m
            INNER JOIN atletas a ON a.id = m.atleta_id
            INNER JOIN filiais f ON f.id = m.filial_id
            {where}
            ORDER BY m.competencia DESC, a.nome_completo
            LIMIT @TamanhoPagina OFFSET @Offset
            """;

        var parametros = new
        {
            FilialId      = filialId,
            AtletaId      = atletaId,
            Status        = status,
            Competencia   = competencia,
            TamanhoPagina = tamanhoPagina,
            Offset        = (pagina - 1) * tamanhoPagina
        };

        await using var conexao = new NpgsqlConnection(_connectionString);
        var total = await conexao.ExecuteScalarAsync<int>(sqlContagem, parametros);
        var itens = await conexao.QueryAsync<MensalidadeResumoDto>(sqlItens, parametros);

        return new ListaMensalidadesDto(itens, total, pagina, tamanhoPagina);
    }
}
