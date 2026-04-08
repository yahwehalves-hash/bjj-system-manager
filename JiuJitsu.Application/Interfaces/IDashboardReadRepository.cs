using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IDashboardReadRepository
{
    Task<DashboardFilialDto> ObterDashboardFilialAsync(
        Guid   filialId,
        string competencia,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DashboardFilialDto>> ObterDashboardConsolidadoAsync(
        string competencia,
        CancellationToken cancellationToken = default);
}
