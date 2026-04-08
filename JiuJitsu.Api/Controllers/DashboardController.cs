using JiuJitsu.Application.Dashboard.Queries.ObterDashboardConsolidado;
using JiuJitsu.Application.Dashboard.Queries.ObterDashboardFilial;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IFilialContexto _filialContexto;

    public DashboardController(IMediator mediator, IFilialContexto filialContexto)
    {
        _mediator        = mediator;
        _filialContexto  = filialContexto;
    }

    /// <summary>Dashboard da filial do usuário logado (ou filialId informado, se Admin).</summary>
    [HttpGet("financeiro")]
    [ProducesResponseType(typeof(DashboardFilialDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterFinanceiro(
        [FromQuery] Guid?   filialId,
        [FromQuery] string? competencia,
        CancellationToken cancellationToken = default)
    {
        var comp = competencia ?? $"{DateTime.UtcNow:yyyy-MM}";
        var fid  = _filialContexto.IsAdmin
            ? (filialId ?? throw new ArgumentException("Informe o filialId para o Admin."))
            : (_filialContexto.FilialId ?? throw new InvalidOperationException("Usuário sem filial vinculada."));

        var resultado = await _mediator.Send(new ObterDashboardFilialQuery(fid, comp), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Dashboard consolidado de toda a rede. Apenas Admin.</summary>
    [HttpGet("financeiro/consolidado")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DashboardConsolidadoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterConsolidado(
        [FromQuery] string? competencia,
        CancellationToken cancellationToken = default)
    {
        var comp = competencia ?? $"{DateTime.UtcNow:yyyy-MM}";
        var resultado = await _mediator.Send(new ObterDashboardConsolidadoQuery(comp), cancellationToken);
        return Ok(resultado);
    }
}
