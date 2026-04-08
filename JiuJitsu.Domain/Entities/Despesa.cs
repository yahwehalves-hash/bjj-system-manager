using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

public class Despesa
{
    public Guid            Id              { get; private set; }
    public Guid            FilialId        { get; private set; }
    public string          Descricao       { get; private set; }
    public CategoriaDespesa Categoria      { get; private set; }
    public string          Subcategoria    { get; private set; }
    public decimal         Valor           { get; private set; }
    public DateOnly        DataCompetencia { get; private set; }
    public DateOnly?       DataPagamento   { get; private set; }
    public StatusDespesa   Status          { get; private set; }
    public FormaPagamento? FormaPagamento  { get; private set; }
    public string?         Observacao      { get; private set; }
    public Guid?           RegistradoPorId { get; private set; }
    public DateTime        CriadoEm       { get; private set; }
    public DateTime?       AtualizadoEm   { get; private set; }

    // Navigation property — preenchida pelo EF Core
    public Filial Filial { get; private set; } = null!;

    private Despesa()
    {
        Descricao    = null!;
        Subcategoria = null!;
    }

    public Despesa(
        Guid            filialId,
        string          descricao,
        CategoriaDespesa categoria,
        string          subcategoria,
        decimal         valor,
        DateOnly        dataCompetencia,
        DateOnly?       dataPagamento,
        FormaPagamento? formaPagamento,
        string?         observacao,
        Guid?           registradoPorId)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição da despesa é obrigatória.");
        if (string.IsNullOrWhiteSpace(subcategoria))
            throw new ArgumentException("Subcategoria da despesa é obrigatória.");
        if (valor <= 0)
            throw new ArgumentException("Valor da despesa deve ser maior que zero.");

        Id              = Guid.CreateVersion7();
        FilialId        = filialId;
        Descricao       = descricao.Trim();
        Categoria       = categoria;
        Subcategoria    = subcategoria.Trim();
        Valor           = valor;
        DataCompetencia = dataCompetencia;
        DataPagamento   = dataPagamento;
        FormaPagamento  = formaPagamento;
        Observacao      = observacao?.Trim();
        RegistradoPorId = registradoPorId;
        Status          = dataPagamento.HasValue ? StatusDespesa.Paga : StatusDespesa.APagar;
        CriadoEm        = DateTime.UtcNow;
    }

    public void MarcarComoPaga(DateOnly dataPagamento, FormaPagamento formaPagamento, string? observacao)
    {
        if (Status == StatusDespesa.Cancelada)
            throw new InvalidOperationException("Despesa cancelada não pode ser paga.");
        if (Status == StatusDespesa.Paga)
            throw new InvalidOperationException("Despesa já foi paga.");

        DataPagamento  = dataPagamento;
        FormaPagamento = formaPagamento;
        Observacao     = observacao ?? Observacao;
        Status         = StatusDespesa.Paga;
        AtualizadoEm   = DateTime.UtcNow;
    }

    public void Cancelar(string? motivo)
    {
        if (Status == StatusDespesa.Paga)
            throw new InvalidOperationException("Despesa paga não pode ser cancelada.");

        Observacao   = motivo;
        Status       = StatusDespesa.Cancelada;
        AtualizadoEm = DateTime.UtcNow;
    }
}