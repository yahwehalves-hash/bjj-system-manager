using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Matriculas.Queries.ListarMatriculas;

public record ListarMatriculasQuery(Guid? AtletaId) : IRequest<IEnumerable<MatriculaDto>>;
