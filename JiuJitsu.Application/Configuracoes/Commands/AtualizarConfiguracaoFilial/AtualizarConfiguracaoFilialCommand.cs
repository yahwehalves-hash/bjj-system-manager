using MediatR;

namespace JiuJitsu.Application.Configuracoes.Commands.AtualizarConfiguracaoFilial;

public record AtualizarConfiguracaoFilialCommand(
    Guid     FilialId,
    decimal? ValorMensalidadePadrao,
    int?     DiaVencimento,
    int?     ToleranciaInadimplenciaDias,
    decimal? MultaAtrasoPercentual,
    decimal? JurosDiarioPercentual,
    decimal? DescontoAntecipacaoPercentual,
    Guid?    UsuarioId) : IRequest;
