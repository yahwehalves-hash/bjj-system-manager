using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.ReadModel;

public class HistoricoGraduacaoReadRepository : IHistoricoGraduacaoReadRepository
{
    private readonly string _connectionString;

    public HistoricoGraduacaoReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    public async Task<HistoricoAtletaDto?> ObterHistoricoAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default)
    {
        const string sqlAtleta = """
            SELECT
                a.id            AS AtletaId,
                a.nome_completo AS NomeCompleto,
                a.foto_base64   AS FotoBase64,
                a.faixa         AS FaixaAtual,
                a.grau          AS GrauAtual
            FROM atletas a
            WHERE a.id = @AtletaId AND a.ativo = true
            """;

        const string sqlHistorico = """
            SELECT
                id          AS Id,
                faixa       AS Faixa,
                grau        AS Grau,
                data_inicio AS DataInicio,
                data_fim    AS DataFim
            FROM historico_graduacoes
            WHERE atleta_id = @AtletaId
            ORDER BY data_inicio
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);

        var atleta = await conexao.QueryFirstOrDefaultAsync<AtletaResumoInterno>(sqlAtleta, new { AtletaId = atletaId });
        if (atleta is null) return null;

        var historico = (await conexao.QueryAsync<HistoricoResumoInterno>(sqlHistorico, new { AtletaId = atletaId })).ToList();

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var historicoDto = historico.Select(h =>
        {
            var fim  = h.DataFim ?? hoje;
            var dias = fim.DayNumber - h.DataInicio.DayNumber;
            return new HistoricoGraduacaoDto(h.Id, h.Faixa, h.Grau, h.DataInicio, h.DataFim, dias);
        }).ToList();

        var totalDias = historicoDto.Sum(h => h.DiasNaGraduacao);

        return new HistoricoAtletaDto(
            AtletaId:        atleta.AtletaId,
            NomeCompleto:    atleta.NomeCompleto,
            FotoBase64:      atleta.FotoBase64,
            FaixaAtual:      atleta.FaixaAtual,
            GrauAtual:       atleta.GrauAtual,
            TotalDiasNaArte: totalDias,
            Historico:       historicoDto);
    }

    private record AtletaResumoInterno(Guid AtletaId, string NomeCompleto, string? FotoBase64, string FaixaAtual, int GrauAtual);
    private record HistoricoResumoInterno(Guid Id, string Faixa, int Grau, DateOnly DataInicio, DateOnly? DataFim);
}
