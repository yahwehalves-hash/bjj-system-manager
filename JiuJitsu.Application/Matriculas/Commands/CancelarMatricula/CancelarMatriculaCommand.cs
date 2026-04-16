using MediatR;

namespace JiuJitsu.Application.Matriculas.Commands.CancelarMatricula;

public record CancelarMatriculaCommand(Guid Id, string? Motivo) : IRequest;
