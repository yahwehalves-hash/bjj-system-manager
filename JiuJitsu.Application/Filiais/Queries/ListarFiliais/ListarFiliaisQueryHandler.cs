using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Filiais.Queries.ListarFiliais;

public class ListarFiliaisQueryHandler : IRequestHandler<ListarFiliaisQuery, IEnumerable<FilialResumoDto>>
{
    private readonly IFilialReadRepository _repo;

    public ListarFiliaisQueryHandler(IFilialReadRepository repo) => _repo = repo;

    public async Task<IEnumerable<FilialResumoDto>> Handle(ListarFiliaisQuery request, CancellationToken cancellationToken)
        => await _repo.ListarAsync(request.Ativo, cancellationToken);
}
