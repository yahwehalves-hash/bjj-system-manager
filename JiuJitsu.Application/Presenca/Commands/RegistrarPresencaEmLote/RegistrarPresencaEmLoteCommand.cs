using MediatR;

namespace JiuJitsu.Application.Presenca.Commands.RegistrarPresencaEmLote;

public record RegistrarPresencaEmLoteCommand(
    Guid        TurmaId,
    Guid        FilialId,
    Guid?       RegistradoPor,
    List<Guid>  AtletaIds) : IRequest<int>;
