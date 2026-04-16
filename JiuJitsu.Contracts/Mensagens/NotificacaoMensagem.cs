namespace JiuJitsu.Contracts.Mensagens;

public class NotificacaoMensagem
{
    public string   Evento      { get; set; } = string.Empty;
    public Guid     AtletaId    { get; set; }
    public string   NomeAtleta  { get; set; } = string.Empty;
    public string?  Telefone    { get; set; }
    public string?  Email       { get; set; }
    public string   NomeAcademia { get; set; } = string.Empty;
    public decimal? Valor       { get; set; }
    public DateOnly? DataVencimento { get; set; }
    public DateTime OcorridoEm  { get; set; }
}
