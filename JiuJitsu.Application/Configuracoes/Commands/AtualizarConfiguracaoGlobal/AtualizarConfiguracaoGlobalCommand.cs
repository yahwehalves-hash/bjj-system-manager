using MediatR;

namespace JiuJitsu.Application.Configuracoes.Commands.AtualizarConfiguracaoGlobal;

public record AtualizarConfiguracaoGlobalCommand(
    decimal ValorMensalidadePadrao,
    int     DiaVencimento,
    int     ToleranciaInadimplenciaDias,
    decimal MultaAtrasoPercentual,
    decimal JurosDiarioPercentual,
    decimal DescontoAntecipacaoPercentual,
    Guid?   UsuarioId) : IRequest;
