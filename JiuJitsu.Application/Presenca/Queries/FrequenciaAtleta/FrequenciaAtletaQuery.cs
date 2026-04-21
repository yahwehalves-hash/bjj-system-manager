using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Presenca.Queries.FrequenciaAtleta;

public record FrequenciaAtletaQuery(
    Guid     AtletaId,
    DateOnly DataInicio,
    DateOnly DataFim) : IRequest<IEnumerable<FrequenciaAtletaDto>>;
