using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Presenca.Queries.FrequenciaAtleta;

public class FrequenciaAtletaQueryHandler : IRequestHandler<FrequenciaAtletaQuery, IEnumerable<FrequenciaAtletaDto>>
{
    private readonly IPresencaReadRepository _read;

    public FrequenciaAtletaQueryHandler(IPresencaReadRepository read) => _read = read;

    public Task<IEnumerable<FrequenciaAtletaDto>> Handle(FrequenciaAtletaQuery request, CancellationToken cancellationToken)
        => _read.FrequenciaPorAtletaAsync(request.AtletaId, request.DataInicio, request.DataFim, cancellationToken);
}
