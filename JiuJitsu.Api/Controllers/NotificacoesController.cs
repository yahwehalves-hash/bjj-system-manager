using System.Net.Http.Json;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificacoesController : ControllerBase
{
    private readonly INotificacaoRepository _repo;
    private readonly IHttpClientFactory     _httpClientFactory;
    private readonly IConfiguration         _config;

    public NotificacoesController(
        INotificacaoRepository repo,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _repo              = repo;
        _httpClientFactory = httpClientFactory;
        _config            = config;
    }

    // ── Templates ────────────────────────────────────────────────────

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

    // ── WhatsApp — Evolution API ──────────────────────────────────────

    /// <summary>Retorna configuração atual da Evolution API.</summary>
    [HttpGet("whatsapp/config")]
    public IActionResult ObterConfig()
    {
        var baseUrl  = _config["EvolutionApi:BaseUrl"];
        var instance = _config["EvolutionApi:Instance"] ?? "jiujitsu";
        return Ok(new
        {
            configurado = !string.IsNullOrWhiteSpace(baseUrl),
            baseUrl,
            instance
        });
    }

    /// <summary>Cria a instância WhatsApp (se não existir) e retorna o QR Code para leitura.</summary>
    [HttpPost("whatsapp/conectar")]
    public async Task<IActionResult> Conectar(CancellationToken cancellationToken)
    {
        var (client, instance) = CriarClientEvolution();
        if (client is null)
            return BadRequest(new { erro = "Evolution API não configurada." });

        // Tenta criar a instância — se já existir, a API retorna erro que ignoramos
        try
        {
            await client.PostAsJsonAsync("/instance/create",
                new { instanceName = instance, qrcode = true, integration = "WHATSAPP-BAILEYS" },
                cancellationToken);
        }
        catch { /* ignora — instância pode já existir */ }

        // Busca QR code
        var response = await client.GetAsync($"/instance/connect/{instance}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, new { erro = "Falha ao obter QR code da Evolution API." });

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
        return Ok(json);
    }

    /// <summary>Retorna o estado de conexão da instância WhatsApp.</summary>
    [HttpGet("whatsapp/status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var (client, instance) = CriarClientEvolution();
        if (client is null)
            return Ok(new { estado = "NAO_CONFIGURADO", conectado = false });

        try
        {
            var response = await client.GetAsync($"/instance/connectionState/{instance}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Ok(new { estado = "ERRO", conectado = false });

            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: cancellationToken);
            return Ok(json);
        }
        catch
        {
            return Ok(new { estado = "EVOLUTION_INDISPONIVEL", conectado = false });
        }
    }

    /// <summary>Envia mensagem de teste diretamente via Evolution API.</summary>
    [HttpPost("whatsapp/testar")]
    public async Task<IActionResult> TestarEnvio(
        [FromBody] TestarWhatsAppRequest request,
        CancellationToken cancellationToken)
    {
        var (client, instance) = CriarClientEvolution();
        if (client is null)
            return BadRequest(new { erro = "Evolution API não configurada." });

        var payload  = new { number = request.Telefone, text = request.Mensagem };
        var response = await client.PostAsJsonAsync($"/message/sendText/{instance}", payload, cancellationToken);

        if (response.IsSuccessStatusCode)
            return Ok(new { enviado = true, mensagem = "Mensagem enviada com sucesso." });

        var erro = await response.Content.ReadAsStringAsync(cancellationToken);
        return BadRequest(new { enviado = false, erro });
    }

    // ── Privado ───────────────────────────────────────────────────────

    private (HttpClient? client, string instance) CriarClientEvolution()
    {
        var baseUrl  = _config["EvolutionApi:BaseUrl"];
        var apiKey   = _config["EvolutionApi:ApiKey"]   ?? string.Empty;
        var instance = _config["EvolutionApi:Instance"] ?? "jiujitsu";

        if (string.IsNullOrWhiteSpace(baseUrl)) return (null, instance);

        var client = _httpClientFactory.CreateClient("evolution");
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Remove("apikey");
        client.DefaultRequestHeaders.Add("apikey", apiKey);

        return (client, instance);
    }
}

public record CriarTemplateRequest(string Evento, string Canal, string Mensagem);
public record AtualizarTemplateRequest(string Mensagem, bool Ativo = true);
public record TestarWhatsAppRequest(string Telefone, string Mensagem);
