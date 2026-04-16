namespace JiuJitsu.Application.DTOs;

public record RelatorioInadimplenciaItemDto(
    string   NomeAtleta,
    string   Cpf,
    string?  Email,
    string   NomeFilial,
    string   Competencia,
    decimal  Valor,
    DateOnly DataVencimento,
    int      DiasAtraso,
    string   Status);

public record RelatorioDreItemDto(
    string  Categoria,
    decimal Valor);

public record RelatorioDreDto(
    string                       Competencia,
    string                       NomeFilial,
    decimal                      ReceitaPrevista,
    decimal                      ReceitaRealizada,
    IEnumerable<RelatorioDreItemDto> Despesas)
{
    public decimal TotalDespesas    => Despesas.Sum(d => d.Valor);
    public decimal ResultadoLiquido => ReceitaRealizada - TotalDespesas;
}

public record RelatorioAtletasPorFaixaItemDto(
    string NomeFilial,
    string Faixa,
    int    Grau,
    int    Total);
