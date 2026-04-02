namespace JiuJitsu.Infrastructure.Email.Configuracoes;

// Configurações de SMTP carregadas do appsettings.json
public class EmailConfiguracoes
{
    public string ServidorSmtp    { get; set; } = "localhost";
    public int    Porta           { get; set; } = 1025;
    public string Remetente       { get; set; } = "noreply@jiujitsu.local";
    public string NomeRemetente   { get; set; } = "Academia JiuJitsu";
}
