using JiuJitsu.Application.Despesas.Commands.CancelarDespesa;
using JiuJitsu.Application.Despesas.Commands.LancarDespesa;
using JiuJitsu.Application.Despesas.Commands.MarcarDespesaComoPaga;
using JiuJitsu.Application.Despesas.Queries.ListarDespesas;
using JiuJitsu.Application.Despesas.Queries.ObterDespesaPorId;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GestorFilial")]
public class DespesasController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IFilialContexto _filialContexto;

    public DespesasController(IMediator mediator, IFilialContexto filialContexto)
    {
        _mediator        = mediator;
        _filialContexto  = filialContexto;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListaDespesasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string?  categoria,
        [FromQuery] string?  status,
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] int      pagina        = 1,
        [FromQuery] int      tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var filialId = _filialContexto.IsAdmin ? null : _filialContexto.FilialId;

        var resultado = await _mediator.Send(
            new ListarDespesasQuery(filialId, categoria, status, dataInicio, dataFim, pagina, tamanhoPagina),
            cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DespesaDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var despesa = await _mediator.Send(new ObterDespesaPorIdQuery(id), cancellationToken);
        return despesa is null ? NotFound() : Ok(despesa);
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Lancar([FromBody] LancarDespesaRequest request, CancellationToken cancellationToken)
    {
        // GestorFilial só pode lançar na sua própria filial
        var filialId = _filialContexto.IsAdmin ? request.FilialId : (_filialContexto.FilialId ?? request.FilialId);
        var usuarioId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (Guid?)null;

        var id = await _mediator.Send(new LancarDespesaCommand(
            filialId,
            request.Descricao,
            request.Categoria,
            request.Subcategoria,
            request.Valor,
            request.DataCompetencia,
            request.DataPagamento,
            request.FormaPagamento,
            request.Observacao,
            usuarioId), cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    [HttpPut("{id:guid}/pagamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarComoPaga(
        Guid id,
        [FromBody] MarcarDespesaComoPagaRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarcarDespesaComoPagaCommand(
            id, request.DataPagamento, request.FormaPagamento, request.Observacao), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(
        Guid id,
        [FromBody] CancelarDespesaRequest? request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelarDespesaCommand(id, request?.Motivo), cancellationToken);
        return NoContent();
    }
}

public record LancarDespesaRequest(
    Guid            FilialId,
    string          Descricao,
    CategoriaDespesa Categoria,
    string          Subcategoria,
    decimal         Valor,
    DateOnly        DataCompetencia,
    DateOnly?       DataPagamento,
    FormaPagamento? FormaPagamento,
    string?         Observacao);

public record MarcarDespesaComoPagaRequest(
    DateOnly      DataPagamento,
    FormaPagamento FormaPagamento,
    string?       Observacao);

public record CancelarDespesaRequest(string? Motivo);
