using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.VincularAtleta;

public record VincularAtletaTurmaCommand(Guid TurmaId, Guid AtletaId) : IRequest;
