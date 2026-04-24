namespace JiuJitsu.Application.DTOs;

public record ConfiguracaoGlobalDto(
    Guid     Id,
    decimal  ValorMensalidadePadrao,
    int      DiaVencimento,
    int      ToleranciaInadimplenciaDias,
    decimal  MultaAtrasoPercentual,
    decimal  JurosDiarioPercentual,
    decimal  DescontoAntecipacaoPercentual,
    string   GatewayTipo,
    bool     GerarCobrancaOnlineAutomatico,
    bool     LembreteInadimplenciaAtivo,
    int      DiasLembreteAposVencimento,
    DateTime AtualizadoEm);

public record ConfiguracaoFilialDto(
    Guid     Id,
    Guid     FilialId,
    decimal? ValorMensalidadePadrao,
    int?     DiaVencimento,
    int?     ToleranciaInadimplenciaDias,
    decimal? MultaAtrasoPercentual,
    decimal? JurosDiarioPercentual,
    decimal? DescontoAntecipacaoPercentual,
    DateTime AtualizadoEm);

public record GatewayDisponivelDto(
    string Nome,
    string Descricao);

/// <summary>Configuração efetiva resolvida: filial com fallback para global.</summary>
public record ConfiguracaoEfetivaDto(
    decimal ValorMensalidadePadrao,
    int     DiaVencimento,
    int     ToleranciaInadimplenciaDias,
    decimal MultaAtrasoPercentual,
    decimal JurosDiarioPercentual,
    decimal DescontoAntecipacaoPercentual);
