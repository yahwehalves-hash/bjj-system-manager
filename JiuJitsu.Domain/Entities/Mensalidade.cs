using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

public class Mensalidade
{
    public Guid              Id             { get; private set; }
    public Guid              AtletaId       { get; private set; }
    public Guid              FilialId       { get; private set; }
    public DateOnly          Competencia    { get; private set; }  // primeiro dia do mês: 2025-01-01
    public decimal           Valor          { get; private set; }
    public decimal?          ValorPago      { get; private set; }
    public DateOnly          DataVencimento { get; private set; }
    public DateOnly?         DataPagamento  { get; private set; }
    public StatusMensalidade Status         { get; private set; }
    public FormaPagamento?   FormaPagamento { get; private set; }
    public string?           Observacao     { get; private set; }
    public DateTime          CriadoEm      { get; private set; }
    public DateTime?         AtualizadoEm  { get; private set; }

    // Navigation properties — preenchidas pelo EF Core
    public Atleta Atleta { get; private set; } = null!;
    public Filial Filial { get; private set; } = null!;

    private Mensalidade() { }

    public Mensalidade(
        Guid    atletaId,
        Guid    filialId,
        DateOnly competencia,
        decimal valor,
        DateOnly dataVencimento)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor da mensalidade deve ser maior que zero.");

        Id             = Guid.CreateVersion7();
        AtletaId       = atletaId;
        FilialId       = filialId;
        Competencia    = competencia;
        Valor          = valor;
        DataVencimento = dataVencimento;
        Status         = StatusMensalidade.Pendente;
        CriadoEm      = DateTime.UtcNow;
    }

    public void RegistrarPagamento(
        decimal       valorPago,
        DateOnly      dataPagamento,
        FormaPagamento formaPagamento,
        string?       observacao)
    {
        if (Status == StatusMensalidade.Cancelada)
            throw new InvalidOperationException("Mensalidade cancelada não pode ser paga.");
        if (valorPago <= 0)
            throw new ArgumentException("Valor pago deve ser maior que zero.");

        ValorPago      = valorPago;
        DataPagamento  = dataPagamento;
        FormaPagamento = formaPagamento;
        Observacao     = observacao;
        Status         = StatusMensalidade.Paga;
        AtualizadoEm   = DateTime.UtcNow;
    }

    public void Negociar(decimal novoValor, DateOnly novaDataVencimento, string? observacao)
    {
        if (Status == StatusMensalidade.Paga || Status == StatusMensalidade.Cancelada)
            throw new InvalidOperationException($"Mensalidade com status '{Status}' não pode ser negociada.");
        if (novoValor <= 0)
            throw new ArgumentException("Novo valor deve ser maior que zero.");

        Valor          = novoValor;
        DataVencimento = novaDataVencimento;
        Observacao     = observacao;
        Status         = StatusMensalidade.Negociada;
        AtualizadoEm   = DateTime.UtcNow;
    }

    // Chamado pelo job diário — primeira transição após vencimento + tolerância
    public void MarcarComoVencida()
    {
        if (Status != StatusMensalidade.Pendente) return;
        Status       = StatusMensalidade.Vencida;
        AtualizadoEm = DateTime.UtcNow;
    }

    // Chamado pelo job diário — após o limite de dias configurado
    public void MarcarComoInadimplente()
    {
        if (Status != StatusMensalidade.Vencida) return;
        Status       = StatusMensalidade.Inadimplente;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Cancelar(string? motivo)
    {
        if (Status == StatusMensalidade.Paga)
            throw new InvalidOperationException("Mensalidade paga não pode ser cancelada.");

        Observacao   = motivo;
        Status       = StatusMensalidade.Cancelada;
        AtualizadoEm = DateTime.UtcNow;
    }
}