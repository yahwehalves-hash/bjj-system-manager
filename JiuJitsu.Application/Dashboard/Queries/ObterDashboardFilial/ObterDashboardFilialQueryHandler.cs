using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Dashboard.Queries.ObterDashboardFilial;

public class ObterDashboardFilialQueryHandler : IRequestHandler<ObterDashboardFilialQuery, DashboardFilialDto>
{
    private readonly IDashboardReadRepository _repo;

    public ObterDashboardFilialQueryHandler(IDashboardReadRepository repo) => _repo = repo;

    public async Task<DashboardFilialDto> Handle(ObterDashboardFilialQuery request, CancellationToken cancellationToken)
        => await _repo.ObterDashboardFilialAsync(request.FilialId, request.Competencia, cancellationToken);
}
