using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class DespesaReadRepository : IDespesaReadRepository
{
    private readonly string _connectionString;

    public DespesaReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<DespesaDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                d.id,
                d.filial_id         AS FilialId,
                f.nome              AS NomeFilial,
                d.descricao,
                d.categoria,
                d.subcategoria,
                d.valor,
                d.data_competencia  AS DataCompetencia,
                d.data_pagamento    AS DataPagamento,
                d.status,
                d.forma_pagamento   AS FormaPagamento,
                d.observacao,
                d.registrado_por_id AS RegistradoPorId,
                d.criado_em         AS CriadoEm,
                d.atualizado_em     AS AtualizadoEm
            FROM despesas d
            INNER JOIN filiais f ON f.id = d.filial_id
            WHERE d.id = @Id
            """;
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstOrDefaultAsync<DespesaDetalheDto>(sql, new { Id = id });
    }

    public async Task<ListaDespesasDto> ListarAsync(
        Guid?    filialId,
        string?  categoria,
        string?  status,
        DateOnly? dataInicio,
        DateOnly? dataFim,
        int      pagina,
        int      tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        var condicoes = new List<string>();
        if (filialId.HasValue)                   condicoes.Add("d.filial_id = @FilialId");
        if (!string.IsNullOrWhiteSpace(categoria)) condicoes.Add("d.categoria = @Categoria");
        if (!string.IsNullOrWhiteSpace(status))  condicoes.Add("d.status = @Status");
        if (dataInicio.HasValue)                 condicoes.Add("d.data_competencia >= @DataInicio");
        if (dataFim.HasValue)                    condicoes.Add("d.data_competencia <= @DataFim");

        var where = condicoes.Count > 0 ? "WHERE " + string.Join(" AND ", condicoes) : string.Empty;

        var sqlContagem = $"SELECT COUNT(*) FROM despesas d {where}";
        var sqlItens = $"""
            SELECT
                d.id,
                d.filial_id         AS FilialId,
                f.nome              AS NomeFilial,
                d.descricao,
                d.categoria,
                d.subcategoria,
                d.valor,
                d.data_competencia  AS DataCompetencia,
                d.data_pagamento    AS DataPagamento,
                d.status,
                d.forma_pagamento   AS FormaPagamento
            FROM despesas d
            INNER JOIN filiais f ON f.id = d.filial_id
            {where}
            ORDER BY d.data_competencia DESC, d.criado_em DESC
            LIMIT @TamanhoPagina OFFSET @Offset
            """;

        var parametros = new
        {
            FilialId      = filialId,
            Categoria     = categoria,
            Status        = status,
            DataInicio    = dataInicio,
            DataFim       = dataFim,
            TamanhoPagina = tamanhoPagina,
            Offset        = (pagina - 1) * tamanhoPagina
        };

        await using var conexao = new NpgsqlConnection(_connectionString);
        var total = await conexao.ExecuteScalarAsync<int>(sqlContagem, parametros);
        var itens = await conexao.QueryAsync<DespesaResumoDto>(sqlItens, parametros);

        return new ListaDespesasDto(itens, total, pagina, tamanhoPagina);
    }
}
