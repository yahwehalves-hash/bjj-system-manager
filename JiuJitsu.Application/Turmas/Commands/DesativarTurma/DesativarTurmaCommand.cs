using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.DesativarTurma;

public record DesativarTurmaCommand(Guid Id) : IRequest;
