using MediatR;

namespace JiuJitsu.Application.Despesas.Commands.CancelarDespesa;

public record CancelarDespesaCommand(Guid DespesaId, string? Motivo) : IRequest;
