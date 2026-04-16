namespace JiuJitsu.Domain.Entities;

/// <summary>Template de mensagem para notificações automáticas.</summary>
public class TemplateNotificacao
{
    public Guid     Id        { get; private set; }
    /// <summary>Ex: "mensalidade.vencendo", "mensalidade.vencida", "aniversario.atleta"</summary>
    public string   Evento    { get; private set; }
    /// <summary>"WhatsApp" ou "Email"</summary>
    public string   Canal     { get; private set; }
    /// <summary>Mensagem com variáveis: {NomeAtleta}, {Valor}, {DataVencimento}, {NomeAcademia}</summary>
    public string   Mensagem  { get; private set; }
    public bool     Ativo     { get; private set; }
    public DateTime CriadoEm  { get; private set; }

    private TemplateNotificacao() { Evento = null!; Canal = null!; Mensagem = null!; }

    public TemplateNotificacao(string evento, string canal, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(evento))   throw new ArgumentException("Evento é obrigatório.");
        if (string.IsNullOrWhiteSpace(mensagem)) throw new ArgumentException("Mensagem é obrigatória.");

        Id       = Guid.CreateVersion7();
        Evento   = evento.Trim();
        Canal    = canal.Trim();
        Mensagem = mensagem.Trim();
        Ativo    = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string mensagem, bool ativo)
    {
        Mensagem = mensagem.Trim();
        Ativo    = ativo;
    }
}
