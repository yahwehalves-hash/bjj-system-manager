using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.NegociarMensalidade;

public record NegociarMensalidadeCommand(
    Guid     MensalidadeId,
    decimal  NovoValor,
    DateOnly NovaDataVencimento,
    string?  Observacao) : IRequest;
