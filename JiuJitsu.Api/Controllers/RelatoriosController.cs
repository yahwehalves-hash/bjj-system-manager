using JiuJitsu.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GestorFilial")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;
    private readonly IFilialContexto   _filialContexto;

    public RelatoriosController(IRelatorioService relatorioService, IFilialContexto filialContexto)
    {
        _relatorioService = relatorioService;
        _filialContexto   = filialContexto;
    }

    /// <summary>Relatório de inadimplência em Excel.</summary>
    [HttpGet("inadimplencia")]
    public async Task<IActionResult> Inadimplencia(
        [FromQuery] string? competencia,
        [FromQuery] Guid?   filialId,
        CancellationToken   cancellationToken = default)
    {
        var comp = competencia ?? $"{DateTime.UtcNow:yyyy-MM}";
        var fid  = _filialContexto.IsAdmin ? filialId : _filialContexto.FilialId;

        var bytes = await _relatorioService.GerarInadimplenciaXlsxAsync(fid, comp, cancellationToken);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"inadimplencia_{comp}.xlsx");
    }

    /// <summary>DRE mensal em Excel.</summary>
    [HttpGet("dre")]
    public async Task<IActionResult> Dre(
        [FromQuery] string? competencia,
        [FromQuery] Guid?   filialId,
        CancellationToken   cancellationToken = default)
    {
        var comp = competencia ?? $"{DateTime.UtcNow:yyyy-MM}";
        var fid  = _filialContexto.IsAdmin ? filialId : _filialContexto.FilialId;

        var bytes = await _relatorioService.GerarDreXlsxAsync(fid, comp, cancellationToken);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"dre_{comp}.xlsx");
    }

    /// <summary>Distribuição de atletas por faixa em Excel.</summary>
    [HttpGet("atletas-por-faixa")]
    public async Task<IActionResult> AtletasPorFaixa(
        [FromQuery] Guid?  filialId,
        CancellationToken  cancellationToken = default)
    {
        var fid = _filialContexto.IsAdmin ? filialId : _filialContexto.FilialId;

        var bytes = await _relatorioService.GerarAtletasPorFaixaXlsxAsync(fid, cancellationToken);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"atletas_por_faixa_{DateTime.UtcNow:yyyy-MM-dd}.xlsx");
    }
}
