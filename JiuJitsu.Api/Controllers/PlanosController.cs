using JiuJitsu.Application.Matriculas.Commands.CancelarMatricula;
using JiuJitsu.Application.Matriculas.Commands.CriarMatricula;
using JiuJitsu.Application.Matriculas.Queries.ListarMatriculas;
using JiuJitsu.Application.Planos.Commands.AtualizarPlano;
using JiuJitsu.Application.Planos.Commands.CriarPlano;
using JiuJitsu.Application.Planos.Commands.DesativarPlano;
using JiuJitsu.Application.Planos.Queries.ListarPlanos;
using JiuJitsu.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GestorFilial")]
public class PlanosController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlanosController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lista planos ativos (globais + da filial).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid? filialId, CancellationToken cancellationToken)
    {
        var planos = await _mediator.Send(new ListarPlanosQuery(filialId), cancellationToken);
        return Ok(planos);
    }

    /// <summary>Cria novo plano.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] SalvarPlanoRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CriarPlanoCommand(
            request.FilialId,
            request.Nome,
            request.Descricao,
            request.Valor,
            Enum.Parse<TipoPeriodicidade>(request.Periodicidade)), cancellationToken);
        return CreatedAtAction(null, new { id }, new { id });
    }

    /// <summary>Atualiza plano existente.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] SalvarPlanoRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarPlanoCommand(
            id,
            request.Nome,
            request.Descricao,
            request.Valor,
            Enum.Parse<TipoPeriodicidade>(request.Periodicidade)), cancellationToken);
        return NoContent();
    }

    /// <summary>Desativa plano.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DesativarPlanoCommand(id), cancellationToken);
        return NoContent();
    }

    // ── Matrículas ──────────────────────────────────────────────────

    /// <summary>Lista matrículas de um atleta.</summary>
    [HttpGet("/api/matriculas")]
    public async Task<IActionResult> ListarMatriculas([FromQuery] Guid? atletaId, CancellationToken cancellationToken)
    {
        var matriculas = await _mediator.Send(new ListarMatriculasQuery(atletaId), cancellationToken);
        return Ok(matriculas);
    }

    /// <summary>Cria nova matrícula para um atleta.</summary>
    [HttpPost("/api/matriculas")]
    public async Task<IActionResult> CriarMatricula([FromBody] CriarMatriculaRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CriarMatriculaCommand(
            request.AtletaId,
            request.PlanoId,
            request.DataInicio,
            request.DataFim,
            request.ValorCustomizado,
            request.Observacao), cancellationToken);
        return CreatedAtAction(null, new { id }, new { id });
    }

    /// <summary>Cancela matrícula.</summary>
    [HttpDelete("/api/matriculas/{id:guid}")]
    public async Task<IActionResult> CancelarMatricula(Guid id, [FromBody] CancelarMatriculaRequest? request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelarMatriculaCommand(id, request?.Motivo), cancellationToken);
        return NoContent();
    }
}

public record SalvarPlanoRequest(
    Guid?   FilialId,
    string  Nome,
    string? Descricao,
    decimal Valor,
    string  Periodicidade);

public record CriarMatriculaRequest(
    Guid     AtletaId,
    Guid     PlanoId,
    DateOnly DataInicio,
    DateOnly? DataFim,
    decimal? ValorCustomizado,
    string?  Observacao);

public record CancelarMatriculaRequest(string? Motivo);
