using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

// Repositório de leitura — usa Dapper para consultas otimizadas (sem overhead do EF Core)
public class AtletaReadRepository : IAtletaReadRepository
{
    private readonly string _connectionString;

    public AtletaReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<AtletaDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                a.id                    AS Id,
                a.filial_id             AS FilialId,
                f.nome                  AS NomeFilial,
                a.nome_completo         AS NomeCompleto,
                a.cpf                   AS Cpf,
                a.data_nascimento       AS DataNascimento,
                a.faixa                 AS Faixa,
                a.grau                  AS Grau,
                a.data_ultima_graduacao AS DataUltimaGraduacao,
                a.email                 AS Email,
                a.ativo                 AS Ativo,
                a.criado_em             AS CriadoEm,
                a.atualizado_em         AS AtualizadoEm
            FROM atletas a
            LEFT JOIN filiais f ON f.id = a.filial_id
            WHERE a.id = @Id AND a.ativo = true
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstOrDefaultAsync<AtletaDetalheDto>(sql, new { Id = id });
    }

    public async Task<ListaAtletasDto> ListarAsync(
        string? nome,
        string? faixa,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        var condicoes = new List<string> { "ativo = true" };

        if (!string.IsNullOrWhiteSpace(nome))
            condicoes.Add("nome_completo ILIKE @Nome");

        if (!string.IsNullOrWhiteSpace(faixa))
            condicoes.Add("faixa = @Faixa");

        var where = string.Join(" AND ", condicoes);

        var sqlContagem = $"SELECT COUNT(*) FROM atletas WHERE {where}";

        var sqlItens = $"""
            SELECT
                id              AS Id,
                filial_id       AS FilialId,
                nome_completo   AS NomeCompleto,
                cpf             AS Cpf,
                faixa           AS Faixa,
                grau            AS Grau,
                ativo           AS Ativo
            FROM atletas
            WHERE {where}
            ORDER BY nome_completo
            LIMIT @TamanhoPagina OFFSET @Offset
            """;

        var parametros = new
        {
            Nome  = string.IsNullOrWhiteSpace(nome) ? null : $"%{nome}%",
            Faixa = faixa,
            TamanhoPagina = tamanhoPagina,
            Offset = (pagina - 1) * tamanhoPagina
        };

        await using var conexao = new NpgsqlConnection(_connectionString);

        var total = await conexao.ExecuteScalarAsync<int>(sqlContagem, parametros);
        var itens = await conexao.QueryAsync<AtletaResumoDto>(sqlItens, parametros);

        return new ListaAtletasDto(itens, total, pagina, tamanhoPagina);
    }

    public async Task<bool> ExistePorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM atletas WHERE id = @Id AND ativo = true";
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
    }
}
