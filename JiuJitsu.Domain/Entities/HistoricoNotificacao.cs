namespace JiuJitsu.Domain.Entities;

public class HistoricoNotificacao
{
    public Guid     Id          { get; private set; }
    public Guid     AtletaId    { get; private set; }
    public string   Evento      { get; private set; }
    public string   Canal       { get; private set; }
    /// <summary>"Enviado", "Falhou"</summary>
    public string   Status      { get; private set; }
    public string?  Detalhe     { get; private set; }
    public DateTime EnviadoEm   { get; private set; }

    private HistoricoNotificacao() { Evento = null!; Canal = null!; Status = null!; }

    public HistoricoNotificacao(Guid atletaId, string evento, string canal, string status, string? detalhe = null)
    {
        Id        = Guid.CreateVersion7();
        AtletaId  = atletaId;
        Evento    = evento;
        Canal     = canal;
        Status    = status;
        Detalhe   = detalhe;
        EnviadoEm = DateTime.UtcNow;
    }
}
