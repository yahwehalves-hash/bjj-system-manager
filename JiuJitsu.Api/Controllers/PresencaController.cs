using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Presenca.Commands.RegistrarPresenca;
using JiuJitsu.Application.Presenca.Commands.RegistrarPresencaEmLote;
using JiuJitsu.Application.Presenca.Queries.FrequenciaAtleta;
using JiuJitsu.Application.Presenca.Queries.FrequenciaTurma;
using JiuJitsu.Application.Presenca.Queries.ListarPresencasTurma;
using JiuJitsu.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GestorFilial,Professor")]
public class PresencaController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IFilialContexto _filialContexto;
    private readonly ILogger<PresencaController> _logger;

    public PresencaController(IMediator mediator, IFilialContexto filialContexto, ILogger<PresencaController> logger)
    {
        _mediator        = mediator;
        _filialContexto  = filialContexto;
        _logger          = logger;
    }

    /// <summary>Registra presença de um atleta em uma turma.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarPresencaRequest request,
        CancellationToken cancellationToken)
    {
        var filialId = _filialContexto.FilialId ?? request.FilialId;
        if (filialId is null)
        {
            _logger.LogWarning("Tentativa de registrar presença sem FilialId. Atleta={AtletaId} Turma={TurmaId}", request.AtletaId, request.TurmaId);
            return BadRequest(new { erro = "FilialId é obrigatório." });
        }

        try
        {
            var id = await _mediator.Send(new RegistrarPresencaCommand(
                request.AtletaId,
                request.TurmaId,
                filialId.Value,
                OrigemPresenca.Manual,
                null), cancellationToken);

            return CreatedAtAction(null, new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
    }

    /// <summary>Registra presença de múltiplos atletas em uma turma (chamada de presença).</summary>
    [HttpPost("lote")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarEmLote(
        [FromBody] RegistrarPresencaEmLoteRequest request,
        CancellationToken cancellationToken)
    {
        var filialId = _filialContexto.FilialId ?? request.FilialId;
        if (filialId is null)
        {
            _logger.LogWarning("Tentativa de registrar chamada em lote sem FilialId. Turma={TurmaId}", request.TurmaId);
            return BadRequest(new { erro = "FilialId é obrigatório." });
        }

        var total = await _mediator.Send(new RegistrarPresencaEmLoteCommand(
            request.TurmaId,
            filialId.Value,
            null,
            request.AtletaIds), cancellationToken);

        return Ok(new { registrados = total });
    }

    /// <summary>Lista presenças de uma turma em um período.</summary>
    [HttpGet("turma/{turmaId:guid}")]
    [ProducesResponseType(typeof(ListaPresencasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorTurma(
        Guid turmaId,
        [FromQuery] DateOnly dataInicio,
        [FromQuery] DateOnly dataFim,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ListarPresencasTurmaQuery(turmaId, dataInicio, dataFim), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Frequência de todos os atletas de uma turma em um período.</summary>
    [HttpGet("turma/{turmaId:guid}/frequencia")]
    [ProducesResponseType(typeof(IEnumerable<FrequenciaAtletaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FrequenciaTurma(
        Guid turmaId,
        [FromQuery] DateOnly dataInicio,
        [FromQuery] DateOnly dataFim,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new FrequenciaTurmaQuery(turmaId, dataInicio, dataFim), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>Frequência de um atleta em todas as suas turmas em um período.</summary>
    [HttpGet("atleta/{atletaId:guid}/frequencia")]
    [ProducesResponseType(typeof(IEnumerable<FrequenciaAtletaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FrequenciaAtleta(
        Guid atletaId,
        [FromQuery] DateOnly dataInicio,
        [FromQuery] DateOnly dataFim,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new FrequenciaAtletaQuery(atletaId, dataInicio, dataFim), cancellationToken);
        return Ok(resultado);
    }
}

public record RegistrarPresencaRequest(
    Guid  AtletaId,
    Guid  TurmaId,
    Guid? FilialId);

public record RegistrarPresencaEmLoteRequest(
    Guid       TurmaId,
    Guid?      FilialId,
    List<Guid> AtletaIds);
