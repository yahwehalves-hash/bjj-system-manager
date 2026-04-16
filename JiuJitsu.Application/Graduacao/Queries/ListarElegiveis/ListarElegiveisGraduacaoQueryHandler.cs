using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Graduacao.Queries.ListarElegiveis;

public class ListarElegiveisGraduacaoQueryHandler : IRequestHandler<ListarElegiveisGraduacaoQuery, IEnumerable<ElegívelGraduacaoDto>>
{
    private readonly IGraduacaoReadRepository _read;

    public ListarElegiveisGraduacaoQueryHandler(IGraduacaoReadRepository read) => _read = read;

    public Task<IEnumerable<ElegívelGraduacaoDto>> Handle(ListarElegiveisGraduacaoQuery request, CancellationToken cancellationToken)
        => _read.ListarElegiveisAsync(request.FilialId, cancellationToken);
}
