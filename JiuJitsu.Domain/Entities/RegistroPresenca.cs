using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

public class RegistroPresenca
{
    public Guid            Id             { get; private set; }
    public Guid            AtletaId       { get; private set; }
    public Guid            TurmaId        { get; private set; }
    public Guid            FilialId       { get; private set; }
    public DateTime        DataHora       { get; private set; }
    public OrigemPresenca  Origem         { get; private set; }
    public Guid?           RegistradoPor  { get; private set; }

    // Navigation
    public Atleta Atleta { get; private set; } = null!;
    public Turma  Turma  { get; private set; } = null!;

    private RegistroPresenca() { }

    public RegistroPresenca(
        Guid           atletaId,
        Guid           turmaId,
        Guid           filialId,
        OrigemPresenca origem,
        Guid?          registradoPor = null)
    {
        Id            = Guid.CreateVersion7();
        AtletaId      = atletaId;
        TurmaId       = turmaId;
        FilialId      = filialId;
        DataHora      = DateTime.UtcNow;
        Origem        = origem;
        RegistradoPor = registradoPor;
    }
}
