using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Entities;

/// <summary>
/// Configuração global da rede — singleton, serve como base para todas as filiais.
/// Campos nulos na ConfiguracaoFilial herdam desta entidade.
/// </summary>
public class ConfiguracaoGlobal
{
    public Guid    Id                             { get; private set; }
    public decimal ValorMensalidadePadrao         { get; private set; }
    public int     DiaVencimento                  { get; private set; }  // 1 a 28
    public int     ToleranciaInadimplenciaDias    { get; private set; }  // dias de carência após vencimento
    public decimal MultaAtrasoPercentual          { get; private set; }  // ex.: 0.0200 = 2%
    public decimal JurosDiarioPercentual          { get; private set; }  // ex.: 0.00033
    public decimal    DescontoAntecipacaoPercentual  { get; private set; }
    public GatewayTipo GatewayTipo                  { get; private set; }
    public bool       GerarCobrancaOnlineAutomatico { get; private set; }
    public bool       LembreteInadimplenciaAtivo    { get; private set; }
    public int        DiasLembreteAposVencimento    { get; private set; }
    public DateTime   AtualizadoEm                 { get; private set; }
    public Guid?      AtualizadoPorId              { get; private set; }

    private ConfiguracaoGlobal() { }

    public ConfiguracaoGlobal(
        decimal    valorMensalidadePadrao,
        int        diaVencimento,
        int        toleranciaInadimplenciaDias,
        decimal    multaAtrasoPercentual,
        decimal    jurosDiarioPercentual,
        decimal    descontoAntecipacaoPercentual,
        GatewayTipo gatewayTipo                  = GatewayTipo.Asaas,
        bool       gerarCobrancaOnlineAutomatico = true,
        bool       lembreteInadimplenciaAtivo    = true,
        int        diasLembreteAposVencimento    = 1)
    {
        Id = Guid.CreateVersion7();
        Atualizar(valorMensalidadePadrao, diaVencimento, toleranciaInadimplenciaDias,
                  multaAtrasoPercentual, jurosDiarioPercentual, descontoAntecipacaoPercentual,
                  gatewayTipo, gerarCobrancaOnlineAutomatico, lembreteInadimplenciaAtivo,
                  diasLembreteAposVencimento, null);
    }

    public void Atualizar(
        decimal    valorMensalidadePadrao,
        int        diaVencimento,
        int        toleranciaInadimplenciaDias,
        decimal    multaAtrasoPercentual,
        decimal    jurosDiarioPercentual,
        decimal    descontoAntecipacaoPercentual,
        GatewayTipo gatewayTipo,
        bool       gerarCobrancaOnlineAutomatico,
        bool       lembreteInadimplenciaAtivo,
        int        diasLembreteAposVencimento,
        Guid?      atualizadoPorId)
    {
        Validar(valorMensalidadePadrao, diaVencimento, toleranciaInadimplenciaDias,
                multaAtrasoPercentual, jurosDiarioPercentual, descontoAntecipacaoPercentual,
                diasLembreteAposVencimento);

        ValorMensalidadePadrao        = valorMensalidadePadrao;
        DiaVencimento                 = diaVencimento;
        ToleranciaInadimplenciaDias   = toleranciaInadimplenciaDias;
        MultaAtrasoPercentual         = multaAtrasoPercentual;
        JurosDiarioPercentual         = jurosDiarioPercentual;
        DescontoAntecipacaoPercentual = descontoAntecipacaoPercentual;
        GatewayTipo                   = gatewayTipo;
        GerarCobrancaOnlineAutomatico = gerarCobrancaOnlineAutomatico;
        LembreteInadimplenciaAtivo    = lembreteInadimplenciaAtivo;
        DiasLembreteAposVencimento    = diasLembreteAposVencimento;
        AtualizadoEm                  = DateTime.UtcNow;
        AtualizadoPorId               = atualizadoPorId;
    }

    private static void Validar(
        decimal valorMensalidadePadrao,
        int     diaVencimento,
        int     toleranciaInadimplenciaDias,
        decimal multaAtrasoPercentual,
        decimal jurosDiarioPercentual,
        decimal descontoAntecipacaoPercentual,
        int     diasLembreteAposVencimento)
    {
        if (valorMensalidadePadrao <= 0)
            throw new ArgumentException("Valor da mensalidade deve ser maior que zero.");
        if (diaVencimento < 1 || diaVencimento > 28)
            throw new ArgumentException("Dia de vencimento deve ser entre 1 e 28.");
        if (toleranciaInadimplenciaDias < 0)
            throw new ArgumentException("Tolerância de inadimplência não pode ser negativa.");
        if (multaAtrasoPercentual < 0)
            throw new ArgumentException("Multa de atraso não pode ser negativa.");
        if (jurosDiarioPercentual < 0)
            throw new ArgumentException("Juros diário não pode ser negativo.");
        if (descontoAntecipacaoPercentual < 0)
            throw new ArgumentException("Desconto de antecipação não pode ser negativo.");
        if (diasLembreteAposVencimento < 0)
            throw new ArgumentException("Dias de lembrete não pode ser negativo.");
    }
}