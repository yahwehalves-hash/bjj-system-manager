using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificacoesController : ControllerBase
{
    private readonly INotificacaoRepository _repo;

    public NotificacoesController(INotificacaoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> ListarTemplates(CancellationToken cancellationToken)
    {
        var templates = await _repo.ListarTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CriarTemplate(
        [FromBody] CriarTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var template = new TemplateNotificacao(request.Evento, request.Canal, request.Mensagem);
        await _repo.AdicionarTemplateAsync(template, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return Ok(new { template.Id });
    }

    [HttpPut("templates/{id:guid}")]
    public async Task<IActionResult> AtualizarTemplate(
        Guid id,
        [FromBody] AtualizarTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var template = await _repo.ObterTemplatePorIdAsync(id, cancellationToken);
        if (template is null) return NotFound();
        template.Atualizar(request.Mensagem, request.Ativo);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("templates/{id:guid}")]
    public async Task<IActionResult> RemoverTemplate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _repo.ObterTemplatePorIdAsync(id, cancellationToken);
        if (template is null) return NotFound();
        await _repo.RemoverTemplateAsync(id, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return NoContent();
    }
}

public record CriarTemplateRequest(string Evento, string Canal, string Mensagem);
public record AtualizarTemplateRequest(string Mensagem, bool Ativo = true);
