using System.Net.Http.Json;
using System.Text.Json;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Infrastructure.Pagamento;

public class AsaasService : IGatewayPagamento
{
    private readonly HttpClient                  _http;
    private readonly ILogger<AsaasService>       _logger;
    private readonly IConfiguracaoRepository     _configuracaoRepo;
    private readonly string?                     _apiKey;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool Configurado => !string.IsNullOrWhiteSpace(_apiKey);

    public AsaasService(
        IHttpClientFactory       httpFactory,
        IConfiguration           config,
        IConfiguracaoRepository  configuracaoRepo,
        ILogger<AsaasService>    logger)
    {
        _logger           = logger;
        _configuracaoRepo = configuracaoRepo;
        _apiKey           = config["Asaas:ApiKey"];

        var baseUrl = config["Asaas:BaseUrl"] ?? "https://sandbox.asaas.com/api/v3";
        if (!baseUrl.EndsWith("/")) baseUrl += "/";

        _http = httpFactory.CreateClient("asaas");
        _http.BaseAddress = new Uri(baseUrl);

        if (!string.IsNullOrWhiteSpace(_apiKey))
            _http.DefaultRequestHeaders.Add("access_token", _apiKey);

        _http.DefaultRequestHeaders.Add("User-Agent", "TrinityJiuJitsu/1.0");
    }

    public async Task<string> ObterOuCriarClienteAsync(
        string            cpf, string nome, string email, string? telefone,
        CanalNotificacao  canalNotificacao = CanalNotificacao.Email,
        CancellationToken cancellationToken = default)
    {
        // Busca cliente existente por CPF
        var busca = await _http.GetAsync($"customers?cpfCnpj={cpf}", cancellationToken);
        if (busca.IsSuccessStatusCode)
        {
            var lista = await busca.Content.ReadFromJsonAsync<AsaasListaClientesResponse>(_jsonOpts, cancellationToken);
            var existente = lista?.Data?.FirstOrDefault();
            if (existente is not null)
            {
                await ConfigurarNotificacoesClienteAsync(existente.Id, canalNotificacao, cancellationToken);
                return existente.Id;
            }
        }

        // Cria novo cliente
        var payload = new AsaasClienteRequest(nome, cpf, email, LimparTelefone(telefone));
        var resp = await _http.PostAsJsonAsync("customers", payload, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var corpo = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Asaas erro ao criar cliente: {Status} — {Corpo}", (int)resp.StatusCode, corpo);
            throw new HttpRequestException($"Asaas [{(int)resp.StatusCode}] criar cliente: {corpo}", null, resp.StatusCode);
        }

        var criado = await resp.Content.ReadFromJsonAsync<AsaasClienteResponse>(_jsonOpts, cancellationToken)
            ?? throw new InvalidOperationException("Resposta inválida ao criar cliente no Asaas.");

        _logger.LogInformation("Cliente Asaas criado: {ClienteId} para CPF {Cpf}", criado.Id, cpf);

        await ConfigurarNotificacoesClienteAsync(criado.Id, canalNotificacao, cancellationToken);

        return criado.Id;
    }

    /// <summary>
    /// Configura as notificações de e-mail do cliente no Asaas:
    /// - PAYMENT_CREATED: aviso ao gerar a cobrança
    /// - PAYMENT_OVERDUE: lembrete diário de inadimplência (1 dia após vencimento)
    /// Chamado automaticamente ao criar um novo cliente.
    /// </summary>
    /// <summary>
    /// Configura as notificações do cliente no Asaas conforme canal preferido:
    /// - Email: só e-mail habilitado
    /// - WhatsApp: só WhatsApp habilitado
    /// - Ambos: e-mail e WhatsApp habilitados
    /// Eventos configurados: PAYMENT_CREATED (imediato) e PAYMENT_OVERDUE (1x/dia após vencimento).
    /// </summary>
    private async Task ConfigurarNotificacoesClienteAsync(
        string clienteId, CanalNotificacao canalNotificacao, CancellationToken ct)
    {
        try
        {
            var resp = await _http.GetAsync($"customers/{clienteId}/notifications", ct);
            if (!resp.IsSuccessStatusCode) return;

            var lista = await resp.Content.ReadFromJsonAsync<AsaasListaNotificacoesResponse>(_jsonOpts, ct);
            if (lista?.Data is null) return;

            var usaEmail    = canalNotificacao is CanalNotificacao.Email    or CanalNotificacao.Ambos;
            var usaWhatsApp = canalNotificacao is CanalNotificacao.WhatsApp or CanalNotificacao.Ambos;

            // Lê configuração do banco — fallback para defaults se ainda não configurado
            var config             = await _configuracaoRepo.ObterGlobalAsync(ct);
            var lembreteAtivo      = config?.LembreteInadimplenciaAtivo    ?? true;
            var diasLembrete       = config?.DiasLembreteAposVencimento    ?? 1;

            var eventosPrioritarios = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "PAYMENT_CREATED", 0 },
            };

            if (lembreteAtivo)
                eventosPrioritarios["PAYMENT_OVERDUE"] = diasLembrete;

            foreach (var notif in lista.Data.Where(n => eventosPrioritarios.ContainsKey(n.Event)))
            {
                var offset = eventosPrioritarios[notif.Event];
                var enabled = notif.Event.Equals("PAYMENT_OVERDUE", StringComparison.OrdinalIgnoreCase)
                    ? lembreteAtivo
                    : true;

                var update = new AsaasNotificacaoUpdateRequest(
                    Enabled:                    enabled,
                    EmailEnabledForCustomer:    usaEmail,
                    WhatsappEnabledForCustomer: usaWhatsApp,
                    ScheduleOffset:             offset);

                await _http.PutAsJsonAsync($"customers/{clienteId}/notifications/{notif.Id}", update, ct);
            }

            _logger.LogInformation(
                "Notificações Asaas configuradas para cliente {ClienteId}: canal={Canal}, lembreteAtivo={Ativo}, diasLembrete={Dias}.",
                clienteId, canalNotificacao, lembreteAtivo, diasLembrete);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Não foi possível configurar notificações Asaas para cliente {ClienteId}.", clienteId);
        }
    }

    public async Task<CobrancaResultado> CriarCobrancaAsync(
        string   clienteId,
        decimal  valor,
        DateOnly dataVencimento,
        string   descricao,
        string   referenciaExterna,
        CancellationToken cancellationToken = default)
    {
        // Asaas não aceita data de vencimento no passado — usa hoje se já venceu
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueDateEfetiva = dataVencimento < hoje ? hoje : dataVencimento;

        var payload = new AsaasCobrancaRequest(
            Customer:          clienteId,
            BillingType:       "PIX",
            Value:             valor,
            DueDate:           dueDateEfetiva.ToString("yyyy-MM-dd"),
            Description:       descricao,
            ExternalReference: referenciaExterna);

        var resp = await _http.PostAsJsonAsync("payments", payload, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var corpo = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Asaas erro ao criar cobrança: {Status} — {Corpo}", (int)resp.StatusCode, corpo);
            throw new HttpRequestException($"Asaas [{(int)resp.StatusCode}] criar cobrança: {corpo}", null, resp.StatusCode);
        }

        var cobranca = await resp.Content.ReadFromJsonAsync<AsaasCobrancaResponse>(_jsonOpts, cancellationToken)
            ?? throw new InvalidOperationException("Resposta inválida ao criar cobrança no Asaas.");

        _logger.LogInformation("Cobrança Asaas criada: {CobrancaId} ref={Referencia}", cobranca.Id, referenciaExterna);

        // Busca o PIX copia-e-cola
        string? pixCopiaCola = null;
        try
        {
            var pixResp = await _http.GetAsync($"payments/{cobranca.Id}/pixQrCode", cancellationToken);
            if (pixResp.IsSuccessStatusCode)
            {
                var pix = await pixResp.Content.ReadFromJsonAsync<AsaasPixResponse>(_jsonOpts, cancellationToken);
                pixCopiaCola = pix?.Payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível obter o PIX para cobrança {CobrancaId}", cobranca.Id);
        }

        return new CobrancaResultado(
            CobrancaId:    cobranca.Id,
            LinkPagamento: cobranca.InvoiceUrl ?? cobranca.BankSlipUrl,
            PixCopiaCola:  pixCopiaCola);
    }

    public async Task<string?> ConsultarStatusCobrancaAsync(
        string cobrancaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var resp = await _http.GetAsync($"payments/{cobrancaId}", cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;

            var cobranca = await resp.Content.ReadFromJsonAsync<AsaasCobrancaResponse>(_jsonOpts, cancellationToken);
            return cobranca?.Status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao consultar status da cobrança {CobrancaId} no Asaas.", cobrancaId);
            return null;
        }
    }

    private static readonly HashSet<string> _statusPagos =
        new(StringComparer.OrdinalIgnoreCase) { "RECEIVED", "CONFIRMED", "RECEIVED_IN_CASH" };

    public async Task<CobrancaResultado?> BuscarCobrancaExistentePorReferenciaAsync(
        string referenciaExterna, CancellationToken cancellationToken = default)
    {
        try
        {
            var resp = await _http.GetAsync($"payments?externalReference={referenciaExterna}", cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;

            var lista    = await resp.Content.ReadFromJsonAsync<AsaasListaCobrancasResponse>(_jsonOpts, cancellationToken);
            var cobranca = lista?.Data?.FirstOrDefault();
            if (cobranca is null) return null;

            // Tenta buscar o PIX copia e cola da cobrança existente
            string? pixCopiaCola = null;
            try
            {
                var pixResp = await _http.GetAsync($"payments/{cobranca.Id}/pixQrCode", cancellationToken);
                if (pixResp.IsSuccessStatusCode)
                {
                    var pix = await pixResp.Content.ReadFromJsonAsync<AsaasPixResponse>(_jsonOpts, cancellationToken);
                    pixCopiaCola = pix?.Payload;
                }
            }
            catch { /* PIX opcional — não impede o retorno */ }

            return new CobrancaResultado(
                CobrancaId:    cobranca.Id,
                LinkPagamento: cobranca.InvoiceUrl ?? cobranca.BankSlipUrl,
                PixCopiaCola:  pixCopiaCola);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao buscar cobrança existente por referência {Referencia}.", referenciaExterna);
            return null;
        }
    }

    public async Task<(string CobrancaId, string Status)?> BuscarPagamentoConfirmadoPorReferenciaAsync(
        string referenciaExterna, CancellationToken cancellationToken = default)
    {
        try
        {
            var resp = await _http.GetAsync($"payments?externalReference={referenciaExterna}", cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;

            var lista = await resp.Content.ReadFromJsonAsync<AsaasListaCobrancasResponse>(_jsonOpts, cancellationToken);
            var pago  = lista?.Data?.FirstOrDefault(p => _statusPagos.Contains(p.Status ?? ""));
            if (pago is null) return null;

            return (pago.Id, pago.Status!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao buscar pagamentos por referência {Referencia} no Asaas.", referenciaExterna);
            return null;
        }
    }

    private static string? LimparTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return null;
        var limpo = new string(telefone.Where(char.IsDigit).ToArray());
        return limpo.Length >= 10 ? limpo : null;
    }
}
