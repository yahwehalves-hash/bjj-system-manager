using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

public class Plano
{
    public Guid               Id             { get; private set; }
    public Guid?              FilialId       { get; private set; }  // null = global
    public string             Nome           { get; private set; }  = null!;
    public string?            Descricao      { get; private set; }
    public decimal            Valor          { get; private set; }
    public TipoPeriodicidade  Periodicidade  { get; private set; }
    public bool               Ativo          { get; private set; }
    public DateTime           CriadoEm      { get; private set; }
    public DateTime?          AtualizadoEm  { get; private set; }

    private Plano() { }

    public Plano(Guid? filialId, string nome, string? descricao, decimal valor, TipoPeriodicidade periodicidade)
    {
        if (valor <= 0) throw new ArgumentException("Valor do plano deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome do plano é obrigatório.");

        Id            = Guid.CreateVersion7();
        FilialId      = filialId;
        Nome          = nome.Trim();
        Descricao     = descricao?.Trim();
        Valor         = valor;
        Periodicidade = periodicidade;
        Ativo         = true;
        CriadoEm     = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string? descricao, decimal valor, TipoPeriodicidade periodicidade)
    {
        if (valor <= 0) throw new ArgumentException("Valor do plano deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome do plano é obrigatório.");

        Nome          = nome.Trim();
        Descricao     = descricao?.Trim();
        Valor         = valor;
        Periodicidade = periodicidade;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo        = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}
