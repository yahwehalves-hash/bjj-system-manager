using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Dashboard.Queries.ObterDashboardFilial;

public record ObterDashboardFilialQuery(Guid FilialId, string Competencia) : IRequest<DashboardFilialDto>;
