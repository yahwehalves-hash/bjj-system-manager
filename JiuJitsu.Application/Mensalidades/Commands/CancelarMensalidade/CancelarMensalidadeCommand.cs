using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.CancelarMensalidade;

public record CancelarMensalidadeCommand(Guid MensalidadeId, string? Motivo) : IRequest;
