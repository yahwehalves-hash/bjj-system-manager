using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.ValueObjects;

namespace JiuJitsu.Domain.Entities;

public class Atleta
{
    public Guid      Id                  { get; private set; }
    public Guid      FilialId            { get; private set; }
    public string    NomeCompleto        { get; private set; }
    public Cpf       Cpf                 { get; private set; }
    public DateOnly  DataNascimento      { get; private set; }
    public Faixa     Faixa               { get; private set; }
    public Grau      Grau                { get; private set; }
    public DateOnly  DataUltimaGraduacao { get; private set; }
    public Email     Email               { get; private set; }
    public string?   Telefone            { get; private set; }

    public string?   FotoBase64   { get; private set; }

    // Soft delete — atletas excluídos são marcados como inativos
    public bool      Ativo        { get; private set; }
    public DateTime  CriadoEm    { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    // Navigation properties — preenchidas pelo EF Core
    public Filial Filial { get; private set; } = null!;
    public ICollection<HistoricoGraduacao> Historico { get; private set; } = [];

    // Construtor privado reservado ao EF Core — null! suprime aviso de nullable
    // pois o EF Core preenche as propriedades via reflection após instanciar
    private Atleta()
    {
        NomeCompleto = null!;
        Cpf          = null!;
        Email        = null!;
    }

    public Atleta(
        Guid     filialId,
        string   nomeCompleto,
        Cpf      cpf,
        DateOnly dataNascimento,
        Faixa    faixa,
        Grau     grau,
        DateOnly dataUltimaGraduacao,
        Email    email,
        string?  telefone = null)
    {
        Id                  = Guid.CreateVersion7();
        FilialId            = filialId;
        NomeCompleto        = nomeCompleto;
        Cpf                 = cpf;
        DataNascimento      = dataNascimento;
        Faixa               = faixa;
        Grau                = grau;
        DataUltimaGraduacao = dataUltimaGraduacao;
        Email               = email;
        Telefone            = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Ativo               = true;
        CriadoEm            = DateTime.UtcNow;

        Validar();
    }

    public void Atualizar(
        string   nomeCompleto,
        DateOnly dataNascimento,
        Faixa    faixa,
        Grau     grau,
        DateOnly dataUltimaGraduacao,
        Email    email,
        string?  telefone = null)
    {
        NomeCompleto        = nomeCompleto;
        DataNascimento      = dataNascimento;
        Faixa               = faixa;
        Grau                = grau;
        DataUltimaGraduacao = dataUltimaGraduacao;
        Email               = email;
        Telefone            = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        AtualizadoEm        = DateTime.UtcNow;

        Validar();
    }

    public void AtualizarFoto(string? fotoBase64) => FotoBase64 = fotoBase64;

    // Marca o atleta como inativo (soft delete)
    public void Desativar() => Ativo = false;

    private void Validar()
    {
        if (string.IsNullOrWhiteSpace(NomeCompleto))
            throw new ArgumentException("Nome completo é obrigatório.");

        if (DataNascimento >= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Data de nascimento deve ser no passado.");

        if (DataUltimaGraduacao > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Data da última graduação não pode ser futura.");
    }
}
