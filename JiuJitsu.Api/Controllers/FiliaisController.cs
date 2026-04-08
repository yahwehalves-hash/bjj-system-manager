using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Filiais.Commands.AtualizarFilial;
using JiuJitsu.Application.Filiais.Commands.CriarFilial;
using JiuJitsu.Application.Filiais.Commands.DesativarFilial;
using JiuJitsu.Application.Filiais.Queries.ListarFiliais;
using JiuJitsu.Application.Filiais.Queries.ObterFilialPorId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FiliaisController : ControllerBase
{
    private readonly IMediator _mediator;

    public FiliaisController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FilialResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool? ativo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarFiliaisQuery(ativo), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FilialDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var filial = await _mediator.Send(new ObterFilialPorIdQuery(id), cancellationToken);
        return filial is null ? NotFound() : Ok(filial);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] CriarFilialRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CriarFilialCommand(request.Nome, request.Endereco, request.Cnpj, request.Telefone),
            cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarFilialRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AtualizarFilialCommand(id, request.Nome, request.Endereco, request.Cnpj, request.Telefone),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DesativarFilialCommand(id), cancellationToken);
        return NoContent();
    }
}

public record CriarFilialRequest(string Nome, string? Endereco, string? Cnpj, string? Telefone);
public record AtualizarFilialRequest(string Nome, string? Endereco, string? Cnpj, string? Telefone);
