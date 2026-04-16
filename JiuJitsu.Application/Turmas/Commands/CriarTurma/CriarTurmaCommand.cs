using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.CriarTurma;

public record CriarTurmaCommand(
    Guid    FilialId,
    string  Nome,
    Guid?   ProfessorId,
    string  DiasSemana,
    string  Horario,
    int     CapacidadeMaxima) : IRequest<Guid>;
