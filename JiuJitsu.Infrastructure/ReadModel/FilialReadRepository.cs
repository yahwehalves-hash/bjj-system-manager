using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class FilialReadRepository : IFilialReadRepository
{
    private readonly string _connectionString;

    public FilialReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<FilialDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, nome, endereco, cnpj, telefone, ativo, criado_em AS CriadoEm
            FROM filiais
            WHERE id = @Id
            """;
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstOrDefaultAsync<FilialDetalheDto>(sql, new { Id = id });
    }

    public async Task<IEnumerable<FilialResumoDto>> ListarAsync(bool? ativo, CancellationToken cancellationToken = default)
    {
        var where = ativo.HasValue ? "WHERE ativo = @Ativo" : string.Empty;
        var sql = $"""
            SELECT id, nome, cnpj, telefone, ativo
            FROM filiais
            {where}
            ORDER BY nome
            """;
        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryAsync<FilialResumoDto>(sql, new { Ativo = ativo });
    }
}
