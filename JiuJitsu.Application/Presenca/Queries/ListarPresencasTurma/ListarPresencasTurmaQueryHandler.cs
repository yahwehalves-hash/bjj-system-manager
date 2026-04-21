using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Presenca.Queries.ListarPresencasTurma;

public class ListarPresencasTurmaQueryHandler : IRequestHandler<ListarPresencasTurmaQuery, ListaPresencasDto>
{
    private readonly IPresencaReadRepository _read;

    public ListarPresencasTurmaQueryHandler(IPresencaReadRepository read) => _read = read;

    public Task<ListaPresencasDto> Handle(ListarPresencasTurmaQuery request, CancellationToken cancellationToken)
        => _read.ListarPorTurmaAsync(request.TurmaId, request.DataInicio, request.DataFim, cancellationToken);
}
