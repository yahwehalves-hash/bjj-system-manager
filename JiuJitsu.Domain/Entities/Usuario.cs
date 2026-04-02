namespace JiuJitsu.Domain.Entities;

public class Usuario
{
    public Guid   Id         { get; private set; }
    public string Nome       { get; private set; }
    public string Email      { get; private set; }
    public string SenhaHash  { get; private set; }
    public string Role       { get; private set; } // Admin | Professor | Aluno
    public DateTime CriadoEm { get; private set; }

    private Usuario() { }

    public Usuario(string nome, string email, string senhaHash, string role)
    {
        if (string.IsNullOrWhiteSpace(nome))  throw new ArgumentException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email é obrigatório.");

        Id        = Guid.NewGuid();
        Nome      = nome;
        Email     = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
        Role      = role;
        CriadoEm = DateTime.UtcNow;
    }
}
