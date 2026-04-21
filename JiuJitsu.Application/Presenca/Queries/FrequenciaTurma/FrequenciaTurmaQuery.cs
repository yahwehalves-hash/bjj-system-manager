using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Presenca.Queries.FrequenciaTurma;

public record FrequenciaTurmaQuery(
    Guid     TurmaId,
    DateOnly DataInicio,
    DateOnly DataFim) : IRequest<IEnumerable<FrequenciaAtletaDto>>;
