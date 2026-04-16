using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Graduacao.Queries.ListarRegras;

public class ListarRegrasGraduacaoQueryHandler : IRequestHandler<ListarRegrasGraduacaoQuery, IEnumerable<RegraGraduacaoDto>>
{
    private readonly IGraduacaoReadRepository _read;

    public ListarRegrasGraduacaoQueryHandler(IGraduacaoReadRepository read) => _read = read;

    public Task<IEnumerable<RegraGraduacaoDto>> Handle(ListarRegrasGraduacaoQuery request, CancellationToken cancellationToken)
        => _read.ListarRegrasAsync(request.FilialId, cancellationToken);
}
