using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Configuracoes.Commands.AtualizarConfiguracaoGlobal;

public record AtualizarConfiguracaoGlobalCommand(
    decimal     ValorMensalidadePadrao,
    int         DiaVencimento,
    int         ToleranciaInadimplenciaDias,
    decimal     MultaAtrasoPercentual,
    decimal     JurosDiarioPercentual,
    decimal     DescontoAntecipacaoPercentual,
    GatewayTipo GatewayTipo,
    bool        GerarCobrancaOnlineAutomatico,
    bool        LembreteInadimplenciaAtivo,
    int         DiasLembreteAposVencimento,
    Guid?       UsuarioId) : IRequest;
