using JiuJitsu.Application.Commands.AdicionarHistoricoManual;
using JiuJitsu.Application.Commands.AtualizarAtleta;
using JiuJitsu.Application.Commands.AtualizarFotoAtleta;
using JiuJitsu.Application.Commands.CriarAtleta;
using JiuJitsu.Application.Commands.ExcluirAtleta;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Queries.ListarAtletas;
using JiuJitsu.Application.Queries.ObterAtletaPorId;
using JiuJitsu.Application.Queries.ObterHistoricoAtleta;
using JiuJitsu.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtletasController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IFilialContexto _filialContexto;

    public AtletasController(IMediator mediator, IFilialContexto filialContexto)
    {
        _mediator       = mediator;
        _filialContexto = filialContexto;
    }

    /// <summary>Lista atletas com filtros opcionais e paginação</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ListaAtletasDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? nome,
        [FromQuery] string? faixa,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _mediator.Send(
            new ListarAtletasQuery(nome, faixa, pagina, tamanhoPagina),
            cancellationToken);

        return Ok(resultado);
    }

    /// <summary>Obtém um atleta pelo ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AtletaDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var atleta = await _mediator.Send(new ObterAtletaPorIdQuery(id), cancellationToken);
        return atleta is null ? NotFound() : Ok(atleta);
    }

    /// <summary>Enfileira a criação de um atleta — retorna 202 Accepted com o ID gerado</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarAtletaRequest request,
        CancellationToken cancellationToken)
    {
        var filialId = _filialContexto.FilialId ?? request.FilialId;

        var command = new CriarAtletaCommand(
            filialId,
            request.NomeCompleto,
            request.Cpf,
            request.DataNascimento,
            request.Faixa,
            request.Grau,
            request.DataUltimaGraduacao,
            request.Email,
            request.Telefone);

        var id = await _mediator.Send(command, cancellationToken);

        return Accepted(new { id, mensagem = "Atleta em processamento. Será salvo em instantes." });
    }

    /// <summary>Enfileira a atualização de um atleta</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarAtletaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AtualizarAtletaCommand(
                id,
                request.NomeCompleto,
                request.DataNascimento,
                request.Faixa,
                request.Grau,
                request.DataUltimaGraduacao,
                request.Email,
                request.Telefone);

            await _mediator.Send(command, cancellationToken);
            return Accepted(new { mensagem = "Atualização em processamento." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Retorna o histórico de graduações de um atleta</summary>
    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(HistoricoAtletaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterHistorico(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterHistoricoAtletaQuery(id), cancellationToken);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    /// <summary>Adiciona um registro manual de histórico de graduação (atletas vindos de outra escola)</summary>
    [HttpPost("{id:guid}/historico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdicionarHistoricoManual(
        Guid id,
        [FromBody] AdicionarHistoricoManualRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(
                new AdicionarHistoricoManualCommand(id, request.Faixa, request.Grau, request.DataInicio, request.DataFim),
                cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Atualiza a foto do atleta (base64)</summary>
    [HttpPut("{id:guid}/foto")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarFoto(
        Guid id,
        [FromBody] AtualizarFotoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new AtualizarFotoAtletaCommand(id, request.FotoBase64), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Enfileira a exclusão (soft delete) de um atleta</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new ExcluirAtletaCommand(id), cancellationToken);
            return Accepted(new { mensagem = "Exclusão em processamento." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

// Request DTOs — usados apenas na camada de apresentação
public record CriarAtletaRequest(
    Guid     FilialId,
    string   NomeCompleto,
    string   Cpf,
    DateOnly DataNascimento,
    Faixa    Faixa,
    Grau     Grau,
    DateOnly DataUltimaGraduacao,
    string   Email,
    string?  Telefone = null
);

public record AtualizarAtletaRequest(
    string   NomeCompleto,
    DateOnly DataNascimento,
    Faixa    Faixa,
    Grau     Grau,
    DateOnly DataUltimaGraduacao,
    string   Email,
    string?  Telefone = null
);

public record AdicionarHistoricoManualRequest(
    Faixa     Faixa,
    Grau      Grau,
    DateOnly  DataInicio,
    DateOnly? DataFim
);

public record AtualizarFotoRequest(string? FotoBase64);
