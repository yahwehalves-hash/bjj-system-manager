using System.Security.Cryptography;
using System.Text;
using Dapper;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JiuJitsu.Infrastructure.Contratos;

public class ContratoService : IContratoService
{
    private readonly IContratoRepository _repo;
    private readonly string              _connectionString;

    public ContratoService(IContratoRepository repo, IConfiguration configuration)
    {
        _repo              = repo;
        _connectionString  = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GerarPdfAsync(Guid atletaId, CancellationToken cancellationToken = default)
    {
        var atleta   = await ObterDadosAtletaAsync(atletaId);
        var template = await _repo.ObterTemplateAtivoAsync(atleta.FilialId, cancellationToken)
                    ?? await _repo.ObterTemplateAtivoAsync(null, cancellationToken);

        var conteudo = template is not null
            ? InterpolaTemplate(template.Conteudo, atleta)
            : GerarConteudoPadrao(atleta);

        return GerarPdf(atleta, conteudo);
    }

    public async Task<Guid> RegistrarAceiteAsync(Guid atletaId, string? ipAceite, CancellationToken cancellationToken = default)
    {
        var pdfBytes = await GerarPdfAsync(atletaId, cancellationToken);
        var hash     = Convert.ToHexString(SHA256.HashData(pdfBytes));

        var atleta   = await ObterDadosAtletaAsync(atletaId);
        var template = await _repo.ObterTemplateAtivoAsync(atleta.FilialId, cancellationToken)
                    ?? await _repo.ObterTemplateAtivoAsync(null, cancellationToken);

        var contrato = new Contrato(atletaId, template?.Id ?? Guid.Empty, hash, ipAceite, pdfBytes);

        await _repo.AdicionarContratoAsync(contrato, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);

        return contrato.Id;
    }

    public async Task<bool> PossuiContratoAsync(Guid atletaId, CancellationToken cancellationToken = default)
    {
        var contrato = await _repo.ObterContratoAtletaAsync(atletaId, cancellationToken);
        return contrato is not null;
    }

    // ── Privados ─────────────────────────────────────────────────────────────

    private async Task<AtletaContratoInfo> ObterDadosAtletaAsync(Guid atletaId)
    {
        const string sql = """
            SELECT
                a.id            AS Id,
                a.filial_id     AS FilialId,
                f.nome          AS NomeFilial,
                a.nome_completo AS NomeCompleto,
                a.cpf           AS Cpf,
                a.data_nascimento AS DataNascimento,
                a.faixa         AS Faixa,
                a.email         AS Email
            FROM atletas a
            INNER JOIN filiais f ON f.id = a.filial_id
            WHERE a.id = @AtletaId
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        return await conexao.QueryFirstOrDefaultAsync<AtletaContratoInfo>(sql, new { AtletaId = atletaId })
            ?? throw new KeyNotFoundException($"Atleta {atletaId} não encontrado.");
    }

    private static string InterpolaTemplate(string template, AtletaContratoInfo a) =>
        template
            .Replace("{NomeAtleta}",   a.NomeCompleto)
            .Replace("{Cpf}",          a.Cpf)
            .Replace("{NomeAcademia}", a.NomeFilial)
            .Replace("{Data}",         DateTime.Today.ToString("dd/MM/yyyy"));

    private static string GerarConteudoPadrao(AtletaContratoInfo a) =>
        $"CONTRATO DE MATRÍCULA\n\nAcademia: {a.NomeFilial}\nAluno: {a.NomeCompleto}\nCPF: {a.Cpf}\n\n" +
        $"Ao assinar este contrato, o aluno declara ciência e concordância com as regras da academia.\n\n" +
        $"Data: {DateTime.Today:dd/MM/yyyy}";

    private static byte[] GerarPdf(AtletaContratoInfo a, string conteudo)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().Text($"CONTRATO DE MATRÍCULA — {a.NomeFilial.ToUpper()}")
                        .FontSize(16).Bold().AlignCenter();
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                    col.Item().PaddingTop(20).Text(conteudo).FontSize(11);
                    col.Item().PaddingTop(40).Text($"Academia: {a.NomeFilial}").FontSize(10);
                    col.Item().PaddingTop(5).Text($"Aluno: {a.NomeCompleto}").FontSize(10);
                    col.Item().PaddingTop(5).Text($"CPF: {a.Cpf}").FontSize(10);
                    col.Item().PaddingTop(5).Text($"Data: {DateTime.Today:dd/MM/yyyy}").FontSize(10);
                    col.Item().PaddingTop(40).Text("_________________________________").AlignCenter();
                    col.Item().Text("Assinatura do Aluno").FontSize(9).AlignCenter();
                });
            });
        });

        return document.GeneratePdf();
    }

    private record AtletaContratoInfo(Guid Id, Guid FilialId, string NomeFilial, string NomeCompleto, string Cpf, DateOnly DataNascimento, string Faixa, string Email);
}
