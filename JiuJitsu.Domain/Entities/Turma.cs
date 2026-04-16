namespace JiuJitsu.Domain.Entities;

public class Turma
{
    public Guid      Id                { get; private set; }
    public Guid      FilialId          { get; private set; }
    public string    Nome              { get; private set; }
    public Guid?     ProfessorId       { get; private set; }
    /// <summary>Dias da semana separados por vírgula. Ex: "Segunda,Quarta,Sexta"</summary>
    public string    DiasSemana        { get; private set; }
    /// <summary>Horário no formato HH:mm. Ex: "19:00"</summary>
    public string    Horario           { get; private set; }
    public int       CapacidadeMaxima  { get; private set; }
    public bool      Ativo             { get; private set; }
    public DateTime  CriadoEm         { get; private set; }
    public DateTime? AtualizadoEm     { get; private set; }

    // Navigation
    public Filial    Filial            { get; private set; } = null!;
    public ICollection<AtletaTurma> AtletasTurmas { get; private set; } = [];

    private Turma() { Nome = null!; DiasSemana = null!; Horario = null!; }

    public Turma(
        Guid    filialId,
        string  nome,
        Guid?   professorId,
        string  diasSemana,
        string  horario,
        int     capacidadeMaxima)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da turma é obrigatório.");
        if (string.IsNullOrWhiteSpace(horario))
            throw new ArgumentException("Horário é obrigatório.");
        if (capacidadeMaxima <= 0)
            throw new ArgumentException("Capacidade máxima deve ser maior que zero.");

        Id               = Guid.CreateVersion7();
        FilialId         = filialId;
        Nome             = nome.Trim();
        ProfessorId      = professorId;
        DiasSemana       = diasSemana.Trim();
        Horario          = horario.Trim();
        CapacidadeMaxima = capacidadeMaxima;
        Ativo            = true;
        CriadoEm        = DateTime.UtcNow;
    }

    public void Atualizar(string nome, Guid? professorId, string diasSemana, string horario, int capacidadeMaxima)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da turma é obrigatório.");
        if (capacidadeMaxima <= 0)
            throw new ArgumentException("Capacidade máxima deve ser maior que zero.");

        Nome             = nome.Trim();
        ProfessorId      = professorId;
        DiasSemana       = diasSemana.Trim();
        Horario          = horario.Trim();
        CapacidadeMaxima = capacidadeMaxima;
        AtualizadoEm    = DateTime.UtcNow;
    }

    public void Desativar() => Ativo = false;
}
