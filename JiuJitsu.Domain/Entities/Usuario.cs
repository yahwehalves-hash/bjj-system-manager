namespace JiuJitsu.Domain.Entities;

public class Usuario
{
    public Guid     Id                { get; private set; }
    public Guid?    FilialId          { get; private set; }  // null = Admin (acesso total à rede)
    public string   Nome              { get; private set; }
    public string   Email             { get; private set; }
    public string   SenhaHash         { get; private set; }
    public string   Role              { get; private set; }  // Admin | GestorFilial | Professor | Aluno
    public bool     DeveAlterarSenha  { get; private set; }
    public DateTime CriadoEm          { get; private set; }

    // Navigation property — preenchida pelo EF Core quando FilialId não é nulo
    public Filial? Filial { get; private set; }

    private Usuario()
    {
        Nome      = null!;
        Email     = null!;
        SenhaHash = null!;
        Role      = null!;
    }

    public Usuario(string nome, string email, string senhaHash, string role, Guid? filialId = null, bool deveAlterarSenha = false)
    {
        if (string.IsNullOrWhiteSpace(nome))  throw new ArgumentException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email é obrigatório.");

        Id               = Guid.CreateVersion7();
        FilialId         = filialId;
        Nome             = nome;
        Email            = email.Trim().ToLowerInvariant();
        SenhaHash        = senhaHash;
        Role             = role;
        DeveAlterarSenha = deveAlterarSenha;
        CriadoEm        = DateTime.UtcNow;
    }

    public void AlterarRole(string novaRole)
    {
        if (string.IsNullOrWhiteSpace(novaRole)) throw new ArgumentException("Role é obrigatória.");
        Role = novaRole;
    }

    public void AlterarSenha(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash)) throw new ArgumentException("Senha inválida.");
        SenhaHash        = novaSenhaHash;
        DeveAlterarSenha = false;
    }
}
