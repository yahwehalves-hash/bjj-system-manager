using JiuJitsu.Application.Configuracoes.Commands.AtualizarConfiguracaoFilial;
using JiuJitsu.Application.Configuracoes.Commands.AtualizarConfiguracaoGlobal;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Application.Configuracoes.Queries.ObterConfiguracaoEfetiva;
using JiuJitsu.Application.Configuracoes.Queries.ObterConfiguracaoGlobal;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracoesController : ControllerBase
{
    private readonly IMediator      _mediator;
    private readonly IFilialContexto _filialContexto;

    private readonly IConfiguracaoReadRepository _readRepo;

    public ConfiguracoesController(IMediator mediator, IFilialContexto filialContexto, IConfiguracaoReadRepository readRepo)
    {
        _mediator        = mediator;
        _filialContexto  = filialContexto;
        _readRepo        = readRepo;
    }

    [HttpGet("gateways-disponiveis")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<GatewayDisponivelDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarGatewaysDisponiveis(CancellationToken cancellationToken)
    {
        var gateways = await _readRepo.ListarGatewaysDisponiveisAsync(cancellationToken);
        return Ok(gateways);
    }

    [HttpGet("global")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ConfiguracaoGlobalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterGlobal(CancellationToken cancellationToken)
    {
        var config = await _mediator.Send(new ObterConfiguracaoGlobalQuery(), cancellationToken);
        return config is null ? NotFound() : Ok(config);
    }

    [HttpPut("global")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AtualizarGlobal(
        [FromBody] AtualizarConfiguracaoGlobalRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        await _mediator.Send(new AtualizarConfiguracaoGlobalCommand(
            request.ValorMensalidadePadrao,
            request.DiaVencimento,
            request.ToleranciaInadimplenciaDias,
            request.MultaAtrasoPercentual,
            request.JurosDiarioPercentual,
            request.DescontoAntecipacaoPercentual,
            request.GatewayTipo,
            request.GerarCobrancaOnlineAutomatico,
            request.LembreteInadimplenciaAtivo,
            request.DiasLembreteAposVencimento,
            usuarioId), cancellationToken);
        return NoContent();
    }

    [HttpGet("filial/{filialId:guid}/efetiva")]
    [ProducesResponseType(typeof(ConfiguracaoEfetivaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterEfetiva(Guid filialId, CancellationToken cancellationToken)
    {
        var config = await _mediator.Send(new ObterConfiguracaoEfetivaQuery(filialId), cancellationToken);
        return Ok(config);
    }

    [HttpPut("filial/{filialId:guid}")]
    [Authorize(Roles = "Admin,GestorFilial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AtualizarFilial(
        Guid filialId,
        [FromBody] AtualizarConfiguracaoFilialRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        await _mediator.Send(new AtualizarConfiguracaoFilialCommand(
            filialId,
            request.ValorMensalidadePadrao,
            request.DiaVencimento,
            request.ToleranciaInadimplenciaDias,
            request.MultaAtrasoPercentual,
            request.JurosDiarioPercentual,
            request.DescontoAntecipacaoPercentual,
            usuarioId), cancellationToken);
        return NoContent();
    }
}

public record AtualizarConfiguracaoGlobalRequest(
    decimal     ValorMensalidadePadrao,
    int         DiaVencimento,
    int         ToleranciaInadimplenciaDias,
    decimal     MultaAtrasoPercentual,
    decimal     JurosDiarioPercentual,
    decimal     DescontoAntecipacaoPercentual,
    GatewayTipo GatewayTipo,
    bool        GerarCobrancaOnlineAutomatico,
    bool        LembreteInadimplenciaAtivo,
    int         DiasLembreteAposVencimento);

public record AtualizarConfiguracaoFilialRequest(
    decimal? ValorMensalidadePadrao,
    int?     DiaVencimento,
    int?     ToleranciaInadimplenciaDias,
    decimal? MultaAtrasoPercentual,
    decimal? JurosDiarioPercentual,
    decimal? DescontoAntecipacaoPercentual);
