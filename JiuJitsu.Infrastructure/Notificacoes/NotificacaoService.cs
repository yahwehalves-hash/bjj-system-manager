using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Messaging.Constantes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace JiuJitsu.Infrastructure.Notificacoes;

public class NotificacaoService : INotificacaoService
{
    private readonly INotificacaoRepository _repo;
    private readonly IEmailService          _emailService;
    private readonly IConnection            _rabbitConnection;
    private readonly IHttpClientFactory     _httpClientFactory;
    private readonly IConfiguration         _config;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(
        INotificacaoRepository repo,
        IEmailService emailService,
        IConnection rabbitConnection,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<NotificacaoService> logger)
    {
        _repo              = repo;
        _emailService      = emailService;
        _rabbitConnection  = rabbitConnection;
        _httpClientFactory = httpClientFactory;
        _config            = config;
        _logger            = logger;
    }

    /// <summary>Publica mensagem de cobrança na fila de notificações.</summary>
    public Task PublicarCobrancaAsync(NotificacaoMensagem mensagem, CancellationToken cancellationToken = default)
    {
        PublicarNaFila(mensagem, RabbitMqConstantes.RoutingNotificacaoCobranca);
        return Task.CompletedTask;
    }

    /// <summary>Processa e envia a notificação (chamado pelo consumer do Worker).</summary>
    public async Task EnviarAsync(NotificacaoMensagem mensagem, CancellationToken cancellationToken = default)
    {
        // Busca template por evento — tenta WhatsApp primeiro, depois Email
        var templateWpp   = await _repo.ObterTemplatePorEventoECanalAsync(mensagem.Evento, "WhatsApp", cancellationToken);
        var templateEmail = await _repo.ObterTemplatePorEventoECanalAsync(mensagem.Evento, "Email", cancellationToken);

        var enviou = false;

        // Tenta WhatsApp se há template, telefone e Evolution API configurada
        if (templateWpp is not null && !string.IsNullOrWhiteSpace(mensagem.Telefone))
        {
            var evolutionUrl = _config["EvolutionApi:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(evolutionUrl))
            {
                var textoWpp = InterpolaMensagem(templateWpp.Mensagem, mensagem);
                enviou = await EnviarWhatsAppAsync(mensagem.Telefone, textoWpp, evolutionUrl, cancellationToken);
            }
        }

        // Fallback Email
        if (!enviou && templateEmail is not null && !string.IsNullOrWhiteSpace(mensagem.Email))
        {
            var textoEmail = InterpolaMensagem(templateEmail.Mensagem, mensagem);
            try
            {
                await _emailService.EnviarAsync(mensagem.Email, $"[{mensagem.NomeAcademia}] {mensagem.Evento}", textoEmail, cancellationToken);
                enviou = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar email para atleta {AtletaId}", mensagem.AtletaId);
            }
        }

        var historico = new HistoricoNotificacao(
            mensagem.AtletaId,
            mensagem.Evento,
            enviou ? (templateWpp is not null ? "WhatsApp" : "Email") : "N/A",
            enviou ? "Enviado" : "Falhou");

        await _repo.RegistrarHistoricoAsync(historico, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }

    // ── Privados ─────────────────────────────────────────────────────────────

    private async Task<bool> EnviarWhatsAppAsync(string telefone, string texto, string baseUrl, CancellationToken ct)
    {
        try
        {
            var instanceName = _config["EvolutionApi:Instance"] ?? "jiujitsu";
            var apiKey       = _config["EvolutionApi:ApiKey"]   ?? string.Empty;

            var client = _httpClientFactory.CreateClient("evolution");
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("apikey", apiKey);

            var payload = new { number = telefone, text = texto };
            var response = await client.PostAsJsonAsync($"/message/sendText/{instanceName}", payload, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar WhatsApp para {Telefone}", telefone);
            return false;
        }
    }

    private void PublicarNaFila(NotificacaoMensagem mensagem, string routingKey)
    {
        using var canal = _rabbitConnection.CreateModel();

        canal.ExchangeDeclare(RabbitMqConstantes.NotificacaoExchangeDlx, ExchangeType.Fanout, durable: true, autoDelete: false);
        canal.QueueDeclare(RabbitMqConstantes.NotificacaoFilaDlq, durable: true, exclusive: false, autoDelete: false);
        canal.QueueBind(RabbitMqConstantes.NotificacaoFilaDlq, RabbitMqConstantes.NotificacaoExchangeDlx, string.Empty);

        canal.ExchangeDeclare(RabbitMqConstantes.NotificacaoExchange, ExchangeType.Direct, durable: true, autoDelete: false);
        canal.QueueDeclare(RabbitMqConstantes.NotificacaoFila, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMqConstantes.NotificacaoExchangeDlx } });
        canal.QueueBind(RabbitMqConstantes.NotificacaoFila, RabbitMqConstantes.NotificacaoExchange, RabbitMqConstantes.RoutingNotificacaoCobranca);
        canal.QueueBind(RabbitMqConstantes.NotificacaoFila, RabbitMqConstantes.NotificacaoExchange, RabbitMqConstantes.RoutingNotificacaoAniversario);

        var json  = JsonSerializer.Serialize(mensagem);
        var corpo = Encoding.UTF8.GetBytes(json);
        var props = canal.CreateBasicProperties();
        props.Persistent  = true;
        props.ContentType = "application/json";

        canal.BasicPublish(RabbitMqConstantes.NotificacaoExchange, routingKey, false, props, corpo);
    }

    private static string InterpolaMensagem(string template, NotificacaoMensagem m) =>
        template
            .Replace("{NomeAtleta}",     m.NomeAtleta)
            .Replace("{NomeAcademia}",   m.NomeAcademia)
            .Replace("{Valor}",          m.Valor?.ToString("C") ?? "")
            .Replace("{DataVencimento}", m.DataVencimento?.ToString("dd/MM/yyyy") ?? "");
}
