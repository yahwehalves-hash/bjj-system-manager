using System.Text.Json.Serialization;

namespace JiuJitsu.Infrastructure.Pagamento;

// ── Clientes ─────────────────────────────────────────────────────────────────

internal record AsaasClienteRequest(
    [property: JsonPropertyName("name")]        string  Name,
    [property: JsonPropertyName("cpfCnpj")]     string  CpfCnpj,
    [property: JsonPropertyName("email")]       string  Email,
    [property: JsonPropertyName("mobilePhone")] string? MobilePhone);

internal record AsaasClienteResponse(
    [property: JsonPropertyName("id")]    string Id,
    [property: JsonPropertyName("name")]  string Name);

internal record AsaasListaClientesResponse(
    [property: JsonPropertyName("data")] IEnumerable<AsaasClienteResponse> Data);

internal record AsaasListaCobrancasResponse(
    [property: JsonPropertyName("data")] IEnumerable<AsaasCobrancaResponse>? Data);

// ── Cobranças ────────────────────────────────────────────────────────────────

internal record AsaasCobrancaRequest(
    [property: JsonPropertyName("customer")]          string  Customer,
    [property: JsonPropertyName("billingType")]       string  BillingType,
    [property: JsonPropertyName("value")]             decimal Value,
    [property: JsonPropertyName("dueDate")]           string  DueDate,
    [property: JsonPropertyName("description")]       string  Description,
    [property: JsonPropertyName("externalReference")] string  ExternalReference);

internal record AsaasCobrancaResponse(
    [property: JsonPropertyName("id")]          string  Id,
    [property: JsonPropertyName("invoiceUrl")]  string? InvoiceUrl,
    [property: JsonPropertyName("bankSlipUrl")] string? BankSlipUrl,
    [property: JsonPropertyName("status")]      string  Status);

internal record AsaasPixResponse(
    [property: JsonPropertyName("encodedImage")] string? EncodedImage,
    [property: JsonPropertyName("payload")]      string? Payload,
    [property: JsonPropertyName("expirationDate")] string? ExpirationDate);

// ── Notificações de cliente ───────────────────────────────────────────────────

internal record AsaasListaNotificacoesResponse(
    [property: JsonPropertyName("data")] IEnumerable<AsaasNotificacaoResponse>? Data);

internal record AsaasNotificacaoResponse(
    [property: JsonPropertyName("id")]                      string Id,
    [property: JsonPropertyName("event")]                   string Event,
    [property: JsonPropertyName("enabled")]                 bool   Enabled,
    [property: JsonPropertyName("emailEnabledForCustomer")] bool   EmailEnabledForCustomer,
    [property: JsonPropertyName("scheduleOffset")]          int    ScheduleOffset);

internal record AsaasNotificacaoUpdateRequest(
    [property: JsonPropertyName("enabled")]                    bool Enabled,
    [property: JsonPropertyName("emailEnabledForCustomer")]    bool EmailEnabledForCustomer,
    [property: JsonPropertyName("whatsappEnabledForCustomer")] bool WhatsappEnabledForCustomer,
    [property: JsonPropertyName("scheduleOffset")]             int  ScheduleOffset);

// ── Webhook ──────────────────────────────────────────────────────────────────

public record AsaasWebhookEvento(
    [property: JsonPropertyName("event")]   string             Event,
    [property: JsonPropertyName("payment")] AsaasWebhookPagamento? Payment);

public record AsaasWebhookPagamento(
    [property: JsonPropertyName("id")]                string  Id,
    [property: JsonPropertyName("status")]            string  Status,
    [property: JsonPropertyName("billingType")]       string  BillingType,
    [property: JsonPropertyName("value")]             decimal Value,
    [property: JsonPropertyName("paymentDate")]       string? PaymentDate,
    [property: JsonPropertyName("externalReference")] string? ExternalReference);
