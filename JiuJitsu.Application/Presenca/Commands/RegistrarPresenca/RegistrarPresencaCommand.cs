using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Presenca.Commands.RegistrarPresenca;

public record RegistrarPresencaCommand(
    Guid           AtletaId,
    Guid           TurmaId,
    Guid           FilialId,
    OrigemPresenca Origem,
    Guid?          RegistradoPor) : IRequest<Guid>;
