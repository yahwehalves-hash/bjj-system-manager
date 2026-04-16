using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class TurmaReadRepository : ITurmaReadRepository
{
    private readonly string _connectionString;

    public TurmaReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<ListaTurmasDto> ListarAsync(
        Guid? filialId, bool? ativo, CancellationToken cancellationToken = default)
    {
        var condicoes = new List<string>();
        if (filialId.HasValue)        condicoes.Add("t.filial_id = @FilialId");
        if (ativo.HasValue)           condicoes.Add("t.ativo = @Ativo");

        var where = condicoes.Count > 0 ? "WHERE " + string.Join(" AND ", condicoes) : string.Empty;

        var sql = $"""
            SELECT
                t.id                        AS Id,
                t.filial_id                 AS FilialId,
                f.nome                      AS NomeFilial,
                t.nome                      AS Nome,
                t.professor_id              AS ProfessorId,
                u.nome                      AS NomeProfessor,
                t.dias_semana               AS DiasSemana,
                t.horario                   AS Horario,
                t.capacidade_maxima         AS CapacidadeMaxima,
                COUNT(att.atleta_id)::int   AS TotalAlunos,
                t.ativo                     AS Ativo
            FROM turmas t
            INNER JOIN filiais f ON f.id = t.filial_id
            LEFT JOIN usuarios u ON u.id = t.professor_id
            LEFT JOIN atletas_turmas att ON att.turma_id = t.id
            {where}
            GROUP BY t.id, f.nome, u.nome
            ORDER BY f.nome, t.horario
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        var itens = (await conexao.QueryAsync<TurmaResumoDto>(
            sql, new { FilialId = filialId, Ativo = ativo })).ToList();

        return new ListaTurmasDto(itens, itens.Count);
    }

    public async Task<TurmaDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sqlTurma = """
            SELECT
                t.id                AS Id,
                t.filial_id         AS FilialId,
                f.nome              AS NomeFilial,
                t.nome              AS Nome,
                t.professor_id      AS ProfessorId,
                u.nome              AS NomeProfessor,
                t.dias_semana       AS DiasSemana,
                t.horario           AS Horario,
                t.capacidade_maxima AS CapacidadeMaxima,
                t.ativo             AS Ativo,
                t.criado_em         AS CriadoEm
            FROM turmas t
            INNER JOIN filiais f ON f.id = t.filial_id
            LEFT JOIN usuarios u ON u.id = t.professor_id
            WHERE t.id = @Id
            """;

        const string sqlAtletas = """
            SELECT
                a.id            AS AtletaId,
                a.nome_completo AS NomeAtleta,
                a.faixa         AS Faixa,
                a.grau          AS Grau,
                at.vinculado_em AS VinculadoEm
            FROM atletas_turmas at
            INNER JOIN atletas a ON a.id = at.atleta_id
            WHERE at.turma_id = @Id
            ORDER BY a.nome_completo
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);

        var turmaRow = await conexao.QueryFirstOrDefaultAsync<dynamic>(sqlTurma, new { Id = id });
        if (turmaRow is null) return null;

        var atletas = await conexao.QueryAsync<TurmaAtletaDto>(sqlAtletas, new { Id = id });

        return new TurmaDetalheDto(
            (Guid)turmaRow.id,
            (Guid)turmaRow.filialid,
            (string)turmaRow.nomefilial,
            (string)turmaRow.nome,
            (Guid?)turmaRow.professorid,
            (string?)turmaRow.nomeprofessor,
            (string)turmaRow.diassemana,
            (string)turmaRow.horario,
            (int)turmaRow.capacidademaxima,
            0,
            (bool)turmaRow.ativo,
            (DateTime)turmaRow.criadoem,
            atletas);
    }
}
