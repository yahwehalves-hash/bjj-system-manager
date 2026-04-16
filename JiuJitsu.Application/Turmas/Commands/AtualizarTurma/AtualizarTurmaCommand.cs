using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.AtualizarTurma;

public record AtualizarTurmaCommand(
    Guid    Id,
    string  Nome,
    Guid?   ProfessorId,
    string  DiasSemana,
    string  Horario,
    int     CapacidadeMaxima) : IRequest;
