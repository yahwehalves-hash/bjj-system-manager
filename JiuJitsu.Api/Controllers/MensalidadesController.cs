using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Mensalidades.Commands.AtualizarStatusVencidas;
using JiuJitsu.Application.Mensalidades.Commands.CancelarMensalidade;
using JiuJitsu.Application.Mensalidades.Commands.GerarMensalidades;
using JiuJitsu.Application.Mensalidades.Commands.NegociarMensalidade;
using JiuJitsu.Application.Mensalidades.Commands.RegistrarPagamento;
using JiuJitsu.Application.Mensalidades.Queries.ListarMensalidades;
using JiuJitsu.Application.Mensalidades.Queries.ObterMensalidadePorId;
using JiuJitsu.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MensalidadesController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IFilialContexto _filialContexto;

    public MensalidadesController(IMediator mediator, IFilialContexto filialContexto)
    {
        _mediator        = mediator;
        _filialContexto  = filialContexto;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListaMensalidadesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid?   atletaId,
        [FromQuery] string? status,
        [FromQuery] string? competencia,
        [FromQuery] int     pagina        = 1,
        [FromQuery] int     tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        // Gestor de filial vê apenas sua filial; Admin pode filtrar qualquer filial
        var filialId = _filialContexto.IsAdmin
            ? (Guid?)null
            : _filialContexto.FilialId;

        var resultado = await _mediator.Send(
            new ListarMensalidadesQuery(filialId, atletaId, status, competencia, pagina, tamanhoPagina),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MensalidadeDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var mensalidade = await _mediator.Send(new ObterMensalidadePorIdQuery(id), cancellationToken);
        return mensalidade is null ? NotFound() : Ok(mensalidade);
    }

    [HttpPost("{id:guid}/pagamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarPagamento(
        Guid id,
        [FromBody] RegistrarPagamentoRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RegistrarPagamentoCommand(
            id,
            request.ValorPago,
            request.DataPagamento,
            request.FormaPagamento,
            request.Observacao), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/negociacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Negociar(
        Guid id,
        [FromBody] NegociarMensalidadeRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new NegociarMensalidadeCommand(
            id,
            request.NovoValor,
            request.NovaDataVencimento,
            request.Observacao), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(
        Guid id,
        [FromBody] CancelarRequest? request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelarMensalidadeCommand(id, request?.Motivo), cancellationToken);
        return NoContent();
    }

    /// <summary>Gera mensalidades manualmente para uma competência. Apenas Admin.</summary>
    [HttpPost("gerar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Gerar(
        [FromBody] GerarMensalidadesRequest request,
        CancellationToken cancellationToken)
    {
        var total = await _mediator.Send(new GerarMensalidadesCommand(request.Competencia), cancellationToken);
        return Ok(new { mensalidadesGeradas = total });
    }
}

public record RegistrarPagamentoRequest(
    decimal       ValorPago,
    DateOnly      DataPagamento,
    FormaPagamento FormaPagamento,
    string?       Observacao);

public record NegociarMensalidadeRequest(
    decimal  NovoValor,
    DateOnly NovaDataVencimento,
    string?  Observacao);

public record CancelarRequest(string? Motivo);
public record GerarMensalidadesRequest(DateOnly Competencia);
