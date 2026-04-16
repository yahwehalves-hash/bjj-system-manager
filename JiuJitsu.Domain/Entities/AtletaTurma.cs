namespace JiuJitsu.Domain.Entities;

public class AtletaTurma
{
    public Guid     AtletaId    { get; private set; }
    public Guid     TurmaId     { get; private set; }
    public DateTime VinculadoEm { get; private set; }

    // Navigation
    public Atleta Atleta { get; private set; } = null!;
    public Turma  Turma  { get; private set; } = null!;

    private AtletaTurma() { }

    public AtletaTurma(Guid atletaId, Guid turmaId)
    {
        AtletaId    = atletaId;
        TurmaId     = turmaId;
        VinculadoEm = DateTime.UtcNow;
    }
}
