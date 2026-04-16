using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Turmas.Commands.AtualizarTurma;
using JiuJitsu.Application.Turmas.Commands.CriarTurma;
using JiuJitsu.Application.Turmas.Commands.DesativarTurma;
using JiuJitsu.Application.Turmas.Commands.DesvincularAtleta;
using JiuJitsu.Application.Turmas.Commands.VincularAtleta;
using JiuJitsu.Application.Turmas.Queries.ListarTurmas;
using JiuJitsu.Application.Turmas.Queries.ObterTurmaPorId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GestorFilial,Professor")]
public class TurmasController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IFilialContexto _filialContexto;

    public TurmasController(IMediator mediator, IFilialContexto filialContexto)
    {
        _mediator        = mediator;
        _filialContexto  = filialContexto;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListaTurmasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] bool? ativo,
        CancellationToken cancellationToken = default)
    {
        var filialId = _filialContexto.IsAdmin ? null : _filialContexto.FilialId;
        var resultado = await _mediator.Send(new ListarTurmasQuery(filialId, ativo ?? true), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TurmaDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var turma = await _mediator.Send(new ObterTurmaPorIdQuery(id), cancellationToken);
        return turma is null ? NotFound() : Ok(turma);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,GestorFilial")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] CriarTurmaRequest request, CancellationToken cancellationToken)
    {
        var filialId = _filialContexto.IsAdmin
            ? (request.FilialId ?? _filialContexto.FilialId ?? Guid.Empty)
            : (_filialContexto.FilialId ?? request.FilialId ?? Guid.Empty);

        if (filialId == Guid.Empty)
            return BadRequest(new { erro = "FilialId é obrigatório." });

        var id = await _mediator.Send(new CriarTurmaCommand(
            filialId, request.Nome, request.ProfessorId, request.DiasSemana, request.Horario, request.CapacidadeMaxima),
            cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,GestorFilial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTurmaRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarTurmaCommand(
            id, request.Nome, request.ProfessorId, request.DiasSemana, request.Horario, request.CapacidadeMaxima),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,GestorFilial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DesativarTurmaCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/atletas/{atletaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VincularAtleta(Guid id, Guid atletaId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new VincularAtletaTurmaCommand(id, atletaId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/atletas/{atletaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DesvincularAtleta(Guid id, Guid atletaId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DesvincularAtletaTurmaCommand(id, atletaId), cancellationToken);
        return NoContent();
    }
}

public record CriarTurmaRequest(
    Guid?   FilialId,
    string  Nome,
    Guid?   ProfessorId,
    string  DiasSemana,
    string  Horario,
    int     CapacidadeMaxima);

public record AtualizarTurmaRequest(
    string  Nome,
    Guid?   ProfessorId,
    string  DiasSemana,
    string  Horario,
    int     CapacidadeMaxima);
