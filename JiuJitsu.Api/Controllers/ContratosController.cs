using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GestorFilial")]
public class ContratosController : ControllerBase
{
    private readonly IContratoService    _contratoService;
    private readonly IContratoRepository _contratoRepo;

    public ContratosController(IContratoService contratoService, IContratoRepository contratoRepo)
    {
        _contratoService = contratoService;
        _contratoRepo    = contratoRepo;
    }

    /// <summary>Gera o PDF de contrato para o atleta (preview).</summary>
    [HttpGet("atletas/{atletaId:guid}/pdf")]
    public async Task<IActionResult> GerarPdf(Guid atletaId, CancellationToken cancellationToken)
    {
        var bytes = await _contratoService.GerarPdfAsync(atletaId, cancellationToken);
        return File(bytes, "application/pdf", $"contrato_{atletaId}.pdf");
    }

    /// <summary>Registra o aceite digital do atleta.</summary>
    [HttpPost("atletas/{atletaId:guid}/aceitar")]
    public async Task<IActionResult> Aceitar(Guid atletaId, CancellationToken cancellationToken)
    {
        var ip          = HttpContext.Connection.RemoteIpAddress?.ToString();
        var contratoId  = await _contratoService.RegistrarAceiteAsync(atletaId, ip, cancellationToken);
        return Ok(new { contratoId });
    }

    /// <summary>Verifica se atleta possui contrato assinado.</summary>
    [HttpGet("atletas/{atletaId:guid}/status")]
    public async Task<IActionResult> Status(Guid atletaId, CancellationToken cancellationToken)
    {
        var possui = await _contratoService.PossuiContratoAsync(atletaId, cancellationToken);
        return Ok(new { assinado = possui });
    }

    /// <summary>Cria ou atualiza template de contrato.</summary>
    [HttpPut("template")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SalvarTemplate(
        [FromBody] SalvarTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var existente = await _contratoRepo.ObterTemplateAtivoAsync(request.FilialId, cancellationToken);
        if (existente is not null)
            existente.AtualizarConteudo(request.Conteudo);
        else
        {
            var novo = new TemplateContrato(request.FilialId, request.Conteudo);
            await _contratoRepo.AdicionarTemplateAsync(novo, cancellationToken);
        }
        await _contratoRepo.SalvarAlteracoesAsync(cancellationToken);
        return NoContent();
    }
}

public record SalvarTemplateRequest(Guid? FilialId, string Conteudo);
