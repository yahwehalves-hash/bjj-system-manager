using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Presenca.Queries.FrequenciaTurma;

public class FrequenciaTurmaQueryHandler : IRequestHandler<FrequenciaTurmaQuery, IEnumerable<FrequenciaAtletaDto>>
{
    private readonly IPresencaReadRepository _read;

    public FrequenciaTurmaQueryHandler(IPresencaReadRepository read) => _read = read;

    public Task<IEnumerable<FrequenciaAtletaDto>> Handle(FrequenciaTurmaQuery request, CancellationToken cancellationToken)
        => _read.FrequenciaPorTurmaAsync(request.TurmaId, request.DataInicio, request.DataFim, cancellationToken);
}
