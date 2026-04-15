using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

public class HistoricoGraduacao
{
    public Guid      Id         { get; private set; }
    public Guid      AtletaId   { get; private set; }
    public Faixa     Faixa      { get; private set; }
    public Grau      Grau       { get; private set; }
    public DateOnly  DataInicio { get; private set; }
    public DateOnly? DataFim    { get; private set; }

    // Navigation property — preenchida pelo EF Core
    public Atleta Atleta { get; private set; } = null!;

    private HistoricoGraduacao() { }

    public HistoricoGraduacao(Guid atletaId, Faixa faixa, Grau grau, DateOnly dataInicio)
    {
        Id         = Guid.CreateVersion7();
        AtletaId   = atletaId;
        Faixa      = faixa;
        Grau       = grau;
        DataInicio = dataInicio;
        DataFim    = null;
    }

    public void Fechar(DateOnly dataFim) => DataFim = dataFim;
}
