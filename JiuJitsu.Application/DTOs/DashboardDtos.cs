namespace JiuJitsu.Application.DTOs;

public record DashboardFilialDto(
    Guid    FilialId,
    string  NomeFilial,
    int     TotalAtletasAtivos,
    int     TotalInadimplentes,
    decimal ReceitaPrevista,
    decimal ReceitaRealizada,
    int     MensalidadesPendentes,
    int     MensalidadesVencidas,
    decimal TotalDespesas)
{
    public decimal ResultadoOperacional => ReceitaRealizada - TotalDespesas;
    public decimal PercentualInadimplencia =>
        TotalAtletasAtivos > 0
            ? Math.Round((decimal)TotalInadimplentes / TotalAtletasAtivos * 100, 2)
            : 0;
}

public record DashboardConsolidadoDto(
    IEnumerable<DashboardFilialDto> Filiais,
    string Competencia)
{
    public decimal TotalReceitaPrevista   => Filiais.Sum(f => f.ReceitaPrevista);
    public decimal TotalReceitaRealizada  => Filiais.Sum(f => f.ReceitaRealizada);
    public decimal TotalDespesas          => Filiais.Sum(f => f.TotalDespesas);
    public decimal ResultadoConsolidado   => TotalReceitaRealizada - TotalDespesas;
    public int     TotalAtletasAtivos     => Filiais.Sum(f => f.TotalAtletasAtivos);
    public int     TotalInadimplentes     => Filiais.Sum(f => f.TotalInadimplentes);
}
