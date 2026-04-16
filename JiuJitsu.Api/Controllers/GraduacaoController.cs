using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Graduacao.Commands.SalvarRegraGraduacao;
using JiuJitsu.Application.Graduacao.Queries.ListarElegiveis;
using JiuJitsu.Application.Graduacao.Queries.ListarRegras;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GestorFilial")]
public class GraduacaoController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IFilialContexto _filialContexto;

    public GraduacaoController(IMediator mediator, IFilialContexto filialContexto)
    {
        _mediator        = mediator;
        _filialContexto  = filialContexto;
    }

    /// <summary>Lista regras de graduação (global ou por filial).</summary>
    [HttpGet("regras")]
    [ProducesResponseType(typeof(IEnumerable<RegraGraduacaoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRegras(
        [FromQuery] Guid? filialId,
        CancellationToken cancellationToken = default)
    {
        var fid = _filialContexto.IsAdmin ? filialId : _filialContexto.FilialId;
        var resultado = await _mediator.Send(new ListarRegrasGraduacaoQuery(fid), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Cria ou atualiza uma regra de graduação.</summary>
    [HttpPut("regras")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SalvarRegra(
        [FromBody] SalvarRegraRequest request,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new SalvarRegraGraduacaoCommand(
            request.FilialId, request.Faixa, request.Grau, request.TempoMinimoMeses),
            cancellationToken);
        return NoContent();
    }

    /// <summary>Lista atletas elegíveis para graduação.</summary>
    [HttpGet("elegiveis")]
    [ProducesResponseType(typeof(IEnumerable<ElegívelGraduacaoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarElegiveis(
        [FromQuery] Guid? filialId,
        CancellationToken cancellationToken = default)
    {
        var fid = _filialContexto.IsAdmin ? filialId : _filialContexto.FilialId;
        var resultado = await _mediator.Send(new ListarElegiveisGraduacaoQuery(fid), cancellationToken);
        return Ok(resultado);
    }
}

public record SalvarRegraRequest(
    Guid?  FilialId,
    Faixa  Faixa,
    Grau   Grau,
    int    TempoMinimoMeses);
