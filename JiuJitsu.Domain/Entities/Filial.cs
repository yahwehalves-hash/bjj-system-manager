namespace JiuJitsu.Domain.Entities;

public class Filial
{
    public Guid    Id        { get; private set; }
    public string  Nome      { get; private set; }
    public string? Endereco  { get; private set; }
    public string? Cnpj      { get; private set; }
    public string? Telefone  { get; private set; }
    public bool    Ativo     { get; private set; }
    public DateTime CriadoEm { get; private set; }

    // Construtor reservado ao EF Core
    private Filial() { Nome = null!; }

    public Filial(string nome, string? endereco, string? cnpj, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da filial é obrigatório.");

        Id       = Guid.CreateVersion7();
        Nome     = nome;
        Endereco = endereco?.Trim();
        Cnpj     = cnpj?.Replace(".", "").Replace("/", "").Replace("-", "");
        Telefone = telefone?.Trim();
        Ativo    = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string? endereco, string? cnpj, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da filial é obrigatório.");

        Nome     = nome;
        Endereco = endereco?.Trim();
        Cnpj     = cnpj?.Replace(".", "").Replace("/", "").Replace("-", "");
        Telefone = telefone?.Trim();
    }

    public void Desativar() => Ativo = false;
}