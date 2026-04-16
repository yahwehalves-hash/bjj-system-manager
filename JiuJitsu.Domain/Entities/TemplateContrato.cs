namespace JiuJitsu.Domain.Entities;

/// <summary>Template HTML de contrato configurável por filial.</summary>
public class TemplateContrato
{
    public Guid     Id        { get; private set; }
    public Guid?    FilialId  { get; private set; }
    public string   Conteudo  { get; private set; }
    public int      Versao    { get; private set; }
    public bool     Ativo     { get; private set; }
    public DateTime CriadoEm  { get; private set; }

    private TemplateContrato() { Conteudo = null!; }

    public TemplateContrato(Guid? filialId, string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException("Conteúdo do contrato é obrigatório.");

        Id       = Guid.CreateVersion7();
        FilialId = filialId;
        Conteudo = conteudo.Trim();
        Versao   = 1;
        Ativo    = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void AtualizarConteudo(string conteudo)
    {
        Conteudo = conteudo.Trim();
        Versao++;
    }

    public void Desativar() => Ativo = false;
}
