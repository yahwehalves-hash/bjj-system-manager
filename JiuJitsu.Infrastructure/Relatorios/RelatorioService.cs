using ClosedXML.Excel;
using Dapper;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace JiuJitsu.Infrastructure.Relatorios;

public class RelatorioService : IRelatorioService
{
    private readonly string _connectionString;

    public RelatorioService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("jiujitsu-db")
            ?? throw new InvalidOperationException("Connection string 'jiujitsu-db' não encontrada.");
    }

    // ── Inadimplência ────────────────────────────────────────────────────────

    public async Task<byte[]> GerarInadimplenciaXlsxAsync(
        Guid? filialId, string competencia, CancellationToken ct = default)
    {
        var condicoes = new List<string>
        {
            "m.status IN ('Vencida','Inadimplente')",
            "TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia"
        };
        if (filialId.HasValue) condicoes.Add("m.filial_id = @FilialId");

        var where = "WHERE " + string.Join(" AND ", condicoes);

        var sql = $"""
            SELECT
                a.nome_completo     AS NomeAtleta,
                a.cpf               AS Cpf,
                a.email             AS Email,
                f.nome              AS NomeFilial,
                TO_CHAR(m.competencia, 'YYYY-MM') AS Competencia,
                m.valor             AS Valor,
                m.data_vencimento   AS DataVencimento,
                (CURRENT_DATE - m.data_vencimento)::int AS DiasAtraso,
                m.status            AS Status
            FROM mensalidades m
            INNER JOIN atletas a ON a.id = m.atleta_id
            INNER JOIN filiais f ON f.id = m.filial_id
            {where}
            ORDER BY f.nome, a.nome_completo
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        var itens = (await conexao.QueryAsync<RelatorioInadimplenciaItemDto>(
            sql, new { FilialId = filialId, Competencia = competencia })).ToList();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Inadimplência");

        // Cabeçalho
        var cabecalhos = new[] { "Atleta", "CPF", "E-mail", "Filial", "Competência", "Valor (R$)", "Vencimento", "Dias Atraso", "Status" };
        for (var i = 0; i < cabecalhos.Length; i++)
        {
            ws.Cell(1, i + 1).Value = cabecalhos[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 73, 125);
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Dados
        for (var r = 0; r < itens.Count; r++)
        {
            var item = itens[r];
            var row  = r + 2;
            ws.Cell(row, 1).Value = item.NomeAtleta;
            ws.Cell(row, 2).Value = item.Cpf;
            ws.Cell(row, 3).Value = item.Email ?? "";
            ws.Cell(row, 4).Value = item.NomeFilial;
            ws.Cell(row, 5).Value = item.Competencia;
            ws.Cell(row, 6).Value = item.Valor;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = item.DataVencimento.ToDateTime(TimeOnly.MinValue);
            ws.Cell(row, 7).Style.NumberFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 8).Value = item.DiasAtraso;
            ws.Cell(row, 9).Value = item.Status;

            if (r % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(235, 241, 250);
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── DRE Mensal ───────────────────────────────────────────────────────────

    public async Task<byte[]> GerarDreXlsxAsync(
        Guid? filialId, string competencia, CancellationToken ct = default)
    {
        var filtroFilial = filialId.HasValue ? "AND f.id = @FilialId" : string.Empty;

        var sqlReceita = $"""
            SELECT
                f.nome AS NomeFilial,
                COALESCE(SUM(m.valor), 0)                               AS ReceitaPrevista,
                COALESCE(SUM(m.valor_pago) FILTER (WHERE m.status = 'Paga'), 0) AS ReceitaRealizada
            FROM filiais f
            LEFT JOIN mensalidades m
                ON m.filial_id = f.id AND TO_CHAR(m.competencia, 'YYYY-MM') = @Competencia
            WHERE f.ativo = true {filtroFilial}
            GROUP BY f.nome
            ORDER BY f.nome
            """;

        var sqlDespesas = $"""
            SELECT
                d.categoria     AS Categoria,
                SUM(d.valor)    AS Valor
            FROM despesas d
            INNER JOIN filiais f ON f.id = d.filial_id
            WHERE TO_CHAR(d.data_competencia, 'YYYY-MM') = @Competencia
              AND d.status != 'Cancelada'
              AND f.ativo = true
              {filtroFilial}
            GROUP BY d.categoria
            ORDER BY d.categoria
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        var receitas  = (await conexao.QueryAsync<dynamic>(sqlReceita,  new { FilialId = filialId, Competencia = competencia })).ToList();
        var despesas  = (await conexao.QueryAsync<RelatorioDreItemDto>(sqlDespesas, new { FilialId = filialId, Competencia = competencia })).ToList();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DRE");

        // Título
        ws.Cell(1, 1).Value = $"DRE — {competencia}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 3).Merge();

        // Receita
        var linha = 3;
        ws.Cell(linha, 1).Value = "RECEITA";
        ws.Cell(linha, 1).Style.Font.Bold = true;
        ws.Cell(linha, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 73, 125);
        ws.Cell(linha, 1).Style.Font.FontColor = XLColor.White;
        ws.Cell(linha, 2).Value = "Prevista (R$)";
        ws.Cell(linha, 2).Style.Font.Bold = true;
        ws.Cell(linha, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 73, 125);
        ws.Cell(linha, 2).Style.Font.FontColor = XLColor.White;
        ws.Cell(linha, 3).Value = "Realizada (R$)";
        ws.Cell(linha, 3).Style.Font.Bold = true;
        ws.Cell(linha, 3).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 73, 125);
        ws.Cell(linha, 3).Style.Font.FontColor = XLColor.White;
        linha++;

        decimal totalPrevista = 0, totalRealizada = 0;
        foreach (var r in receitas)
        {
            ws.Cell(linha, 1).Value = (string)r.nomefilial;
            ws.Cell(linha, 2).Value = (decimal)r.receitaprevista;
            ws.Cell(linha, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(linha, 3).Value = (decimal)r.receitarealizada;
            ws.Cell(linha, 3).Style.NumberFormat.Format = "#,##0.00";
            totalPrevista  += (decimal)r.receitaprevista;
            totalRealizada += (decimal)r.receitarealizada;
            linha++;
        }

        ws.Cell(linha, 1).Value = "TOTAL RECEITA";
        ws.Cell(linha, 1).Style.Font.Bold = true;
        ws.Cell(linha, 2).Value = totalPrevista;
        ws.Cell(linha, 2).Style.Font.Bold = true;
        ws.Cell(linha, 2).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(linha, 3).Value = totalRealizada;
        ws.Cell(linha, 3).Style.Font.Bold = true;
        ws.Cell(linha, 3).Style.NumberFormat.Format = "#,##0.00";
        linha += 2;

        // Despesas
        ws.Cell(linha, 1).Value = "DESPESAS";
        ws.Cell(linha, 1).Style.Font.Bold = true;
        ws.Cell(linha, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(192, 0, 0);
        ws.Cell(linha, 1).Style.Font.FontColor = XLColor.White;
        ws.Cell(linha, 2).Value = "Valor (R$)";
        ws.Cell(linha, 2).Style.Font.Bold = true;
        ws.Cell(linha, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(192, 0, 0);
        ws.Cell(linha, 2).Style.Font.FontColor = XLColor.White;
        linha++;

        decimal totalDespesas = 0;
        foreach (var d in despesas)
        {
            ws.Cell(linha, 1).Value = d.Categoria;
            ws.Cell(linha, 2).Value = d.Valor;
            ws.Cell(linha, 2).Style.NumberFormat.Format = "#,##0.00";
            totalDespesas += d.Valor;
            linha++;
        }

        ws.Cell(linha, 1).Value = "TOTAL DESPESAS";
        ws.Cell(linha, 1).Style.Font.Bold = true;
        ws.Cell(linha, 2).Value = totalDespesas;
        ws.Cell(linha, 2).Style.Font.Bold = true;
        ws.Cell(linha, 2).Style.NumberFormat.Format = "#,##0.00";
        linha += 2;

        // Resultado
        var resultado = totalRealizada - totalDespesas;
        ws.Cell(linha, 1).Value = "RESULTADO OPERACIONAL";
        ws.Cell(linha, 1).Style.Font.Bold = true;
        ws.Cell(linha, 2).Value = resultado;
        ws.Cell(linha, 2).Style.Font.Bold = true;
        ws.Cell(linha, 2).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(linha, 2).Style.Font.FontColor = resultado >= 0 ? XLColor.Green : XLColor.Red;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Atletas por Faixa ────────────────────────────────────────────────────

    public async Task<byte[]> GerarAtletasPorFaixaXlsxAsync(
        Guid? filialId, CancellationToken ct = default)
    {
        var filtroFilial = filialId.HasValue ? "AND f.id = @FilialId" : string.Empty;

        var sql = $"""
            SELECT
                f.nome  AS NomeFilial,
                a.faixa AS Faixa,
                a.grau  AS Grau,
                COUNT(*) AS Total
            FROM atletas a
            INNER JOIN filiais f ON f.id = a.filial_id
            WHERE a.ativo = true {filtroFilial}
            GROUP BY f.nome, a.faixa, a.grau
            ORDER BY f.nome, a.faixa, a.grau
            """;

        await using var conexao = new NpgsqlConnection(_connectionString);
        var itens = (await conexao.QueryAsync<RelatorioAtletasPorFaixaItemDto>(
            sql, new { FilialId = filialId })).ToList();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Atletas por Faixa");

        var cabecalhos = new[] { "Filial", "Faixa", "Grau", "Total Atletas" };
        for (var i = 0; i < cabecalhos.Length; i++)
        {
            ws.Cell(1, i + 1).Value = cabecalhos[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 73, 125);
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        for (var r = 0; r < itens.Count; r++)
        {
            var item = itens[r];
            var row  = r + 2;
            ws.Cell(row, 1).Value = item.NomeFilial;
            ws.Cell(row, 2).Value = item.Faixa;
            ws.Cell(row, 3).Value = item.Grau;
            ws.Cell(row, 4).Value = item.Total;

            if (r % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(235, 241, 250);
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
