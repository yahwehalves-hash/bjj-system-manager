using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Presenca.Queries.ListarPresencasTurma;

public record ListarPresencasTurmaQuery(
    Guid     TurmaId,
    DateOnly DataInicio,
    DateOnly DataFim) : IRequest<ListaPresencasDto>;
