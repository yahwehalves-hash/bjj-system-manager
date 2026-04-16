using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

public class Matricula
{
    public Guid            Id               { get; private set; }
    public Guid            AtletaId         { get; private set; }
    public Guid            PlanoId          { get; private set; }
    public DateOnly        DataInicio       { get; private set; }
    public DateOnly?       DataFim          { get; private set; }
    public decimal?        ValorCustomizado { get; private set; }  // sobrescreve valor do plano
    public StatusMatricula Status           { get; private set; }
    public string?         Observacao       { get; private set; }
    public DateTime        CriadoEm        { get; private set; }
    public DateTime?       AtualizadoEm    { get; private set; }

    // Navigation properties
    public Atleta Atleta { get; private set; } = null!;
    public Plano  Plano  { get; private set; } = null!;

    private Matricula() { }

    public Matricula(Guid atletaId, Guid planoId, DateOnly dataInicio, DateOnly? dataFim, decimal? valorCustomizado, string? observacao)
    {
        Id               = Guid.CreateVersion7();
        AtletaId         = atletaId;
        PlanoId          = planoId;
        DataInicio       = dataInicio;
        DataFim          = dataFim;
        ValorCustomizado = valorCustomizado;
        Observacao       = observacao?.Trim();
        Status           = StatusMatricula.Ativa;
        CriadoEm        = DateTime.UtcNow;
    }

    public void Cancelar(string? motivo)
    {
        if (Status != StatusMatricula.Ativa)
            throw new InvalidOperationException("Apenas matrículas ativas podem ser canceladas.");

        Status       = StatusMatricula.Cancelada;
        Observacao   = motivo;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Expirar()
    {
        if (Status != StatusMatricula.Ativa) return;
        Status       = StatusMatricula.Expirada;
        AtualizadoEm = DateTime.UtcNow;
    }

    public decimal ValorEfetivo(decimal valorPadraoPlan) => ValorCustomizado ?? valorPadraoPlan;
}
