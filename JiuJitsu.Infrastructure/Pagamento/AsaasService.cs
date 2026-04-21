using System.Net.Http.Json;
using System.Text.Json;
using JiuJitsu.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Infrastructure.Pagamento;

public class AsaasService : IGatewayPagamento
{
    private readonly HttpClient              _http;
    private readonly ILogger<AsaasService>   _logger;
    private readonly string?                 _apiKey;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool Configurado => !string.IsNullOrWhiteSpace(_apiKey);

    public AsaasService(
        IHttpClientFactory    httpFactory,
        IConfiguration        config,
        ILogger<AsaasService> logger)
    {
        _logger = logger;
        _apiKey = config["Asaas:ApiKey"];

        var baseUrl = config["Asaas:BaseUrl"] ?? "https://sandbox.asaas.com/api/v3";
        if (!baseUrl.EndsWith("/")) baseUrl += "/";

        _http = httpFactory.CreateClient("asaas");
        _http.BaseAddress = new Uri(baseUrl);

        if (!string.IsNullOrWhiteSpace(_apiKey))
            _http.DefaultRequestHeaders.Add("access_token", _apiKey);

        _http.DefaultRequestHeaders.Add("User-Agent", "TrinityJiuJitsu/1.0");
    }

    public async Task<string> ObterOuCriarClienteAsync(
        string cpf, string nome, string email, string? telefone,
        CancellationToken cancellationToken = default)
    {
        // Busca cliente existente por CPF
        var busca = await _http.GetAsync($"customers?cpfCnpj={cpf}", cancellationToken);
        if (busca.IsSuccessStatusCode)
        {
            var lista = await busca.Content.ReadFromJsonAsync<AsaasListaClientesResponse>(_jsonOpts, cancellationToken);
            var existente = lista?.Data?.FirstOrDefault();
            if (existente is not null)
                return existente.Id;
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
        return criado.Id;
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
            _logger.LogWarning(ex, "Não foi possível consultar status da cobrança {CobrancaId}", cobrancaId);
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
