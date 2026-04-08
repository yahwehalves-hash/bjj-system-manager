namespace JiuJitsu.Domain.Entities;

/// <summary>
/// Override de configuração por filial. Campos nulos indicam herança da ConfiguracaoGlobal.
/// </summary>
public class ConfiguracaoFilial
{
    public Guid     Id                            { get; private set; }
    public Guid     FilialId                      { get; private set; }
    public decimal? ValorMensalidadePadrao        { get; private set; }
    public int?     DiaVencimento                 { get; private set; }
    public int?     ToleranciaInadimplenciaDias   { get; private set; }
    public decimal? MultaAtrasoPercentual         { get; private set; }
    public decimal? JurosDiarioPercentual         { get; private set; }
    public decimal? DescontoAntecipacaoPercentual { get; private set; }
    public DateTime AtualizadoEm                 { get; private set; }
    public Guid?    AtualizadoPorId              { get; private set; }

    // Navigation property — preenchida pelo EF Core
    public Filial Filial { get; private set; } = null!;

    private ConfiguracaoFilial() { }

    public ConfiguracaoFilial(Guid filialId)
    {
        Id           = Guid.CreateVersion7();
        FilialId     = filialId;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Atualizar(
        decimal? valorMensalidadePadrao,
        int?     diaVencimento,
        int?     toleranciaInadimplenciaDias,
        decimal? multaAtrasoPercentual,
        decimal? jurosDiarioPercentual,
        decimal? descontoAntecipacaoPercentual,
        Guid?    atualizadoPorId)
    {
        if (diaVencimento.HasValue && (diaVencimento < 1 || diaVencimento > 28))
            throw new ArgumentException("Dia de vencimento deve ser entre 1 e 28.");
        if (toleranciaInadimplenciaDias.HasValue && toleranciaInadimplenciaDias < 0)
            throw new ArgumentException("Tolerância de inadimplência não pode ser negativa.");

        ValorMensalidadePadrao        = valorMensalidadePadrao;
        DiaVencimento                 = diaVencimento;
        ToleranciaInadimplenciaDias   = toleranciaInadimplenciaDias;
        MultaAtrasoPercentual         = multaAtrasoPercentual;
        JurosDiarioPercentual         = jurosDiarioPercentual;
        DescontoAntecipacaoPercentual = descontoAntecipacaoPercentual;
        AtualizadoEm                  = DateTime.UtcNow;
        AtualizadoPorId               = atualizadoPorId;
    }
}