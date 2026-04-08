using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Dashboard.Queries.ObterDashboardConsolidado;

public record ObterDashboardConsolidadoQuery(string Competencia) : IRequest<DashboardConsolidadoDto>;
