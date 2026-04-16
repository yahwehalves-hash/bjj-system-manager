using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.DesvincularAtleta;

public record DesvincularAtletaTurmaCommand(Guid TurmaId, Guid AtletaId) : IRequest;
