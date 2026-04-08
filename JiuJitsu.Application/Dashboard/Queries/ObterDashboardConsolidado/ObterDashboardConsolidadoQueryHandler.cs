using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Dashboard.Queries.ObterDashboardConsolidado;

public class ObterDashboardConsolidadoQueryHandler : IRequestHandler<ObterDashboardConsolidadoQuery, DashboardConsolidadoDto>
{
    private readonly IDashboardReadRepository _repo;

    public ObterDashboardConsolidadoQueryHandler(IDashboardReadRepository repo) => _repo = repo;

    public async Task<DashboardConsolidadoDto> Handle(ObterDashboardConsolidadoQuery request, CancellationToken cancellationToken)
    {
        var filiais = await _repo.ObterDashboardConsolidadoAsync(request.Competencia, cancellationToken);
        return new DashboardConsolidadoDto(filiais, request.Competencia);
    }
}
