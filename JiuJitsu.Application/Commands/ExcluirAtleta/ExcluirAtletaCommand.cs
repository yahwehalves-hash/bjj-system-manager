using MediatR;

namespace JiuJitsu.Application.Commands.ExcluirAtleta;

public record ExcluirAtletaCommand(Guid Id) : IRequest;
