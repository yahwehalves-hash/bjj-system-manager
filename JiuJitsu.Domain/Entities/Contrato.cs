namespace JiuJitsu.Domain.Entities;

public class Contrato
{
    public Guid     Id              { get; private set; }
    public Guid     AtletaId        { get; private set; }
    public Guid     TemplateId      { get; private set; }
    public string   HashDocumento   { get; private set; }
    public string?  IpAceite        { get; private set; }
    public DateTime DataAceite      { get; private set; }
    public byte[]   PdfBytes        { get; private set; }

    private Contrato() { HashDocumento = null!; PdfBytes = null!; }

    public Contrato(Guid atletaId, Guid templateId, string hashDocumento, string? ipAceite, byte[] pdfBytes)
    {
        Id            = Guid.CreateVersion7();
        AtletaId      = atletaId;
        TemplateId    = templateId;
        HashDocumento = hashDocumento;
        IpAceite      = ipAceite;
        DataAceite    = DateTime.UtcNow;
        PdfBytes      = pdfBytes;
    }
}
