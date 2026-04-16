using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

/// <summary>
/// Define o tempo mínimo (em meses) para progressão entre faixas/graus.
/// FilialId nulo = regra global da rede.
/// </summary>
public class RegraGraduacao
{
    public Guid   Id                 { get; private set; }
    public Guid?  FilialId           { get; private set; }
    public Faixa  Faixa              { get; private set; }
    public Grau   Grau               { get; private set; }
    public int    TempoMinimoMeses   { get; private set; }
    public DateTime AtualizadoEm    { get; private set; }

    private RegraGraduacao() { }

    public RegraGraduacao(Guid? filialId, Faixa faixa, Grau grau, int tempoMinimoMeses)
    {
        if (tempoMinimoMeses < 0)
            throw new ArgumentException("Tempo mínimo não pode ser negativo.");

        Id               = Guid.CreateVersion7();
        FilialId         = filialId;
        Faixa            = faixa;
        Grau             = grau;
        TempoMinimoMeses = tempoMinimoMeses;
        AtualizadoEm    = DateTime.UtcNow;
    }

    public void AtualizarTempo(int tempoMinimoMeses)
    {
        if (tempoMinimoMeses < 0)
            throw new ArgumentException("Tempo mínimo não pode ser negativo.");

        TempoMinimoMeses = tempoMinimoMeses;
        AtualizadoEm    = DateTime.UtcNow;
    }
}
