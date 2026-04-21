using JiuJitsu.Application.Pagamento.Commands.ConfirmarPagamentoOnline;
using JiuJitsu.Infrastructure.Pagamento;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JiuJitsu.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IMediator       _mediator;
    private readonly IConfiguration  _config;

    public WebhookController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _config   = config;
    }

    /// <summary>
    /// Recebe eventos do Asaas (PAYMENT_RECEIVED, PAYMENT_CONFIRMED).
    /// Valida o token de autenticação configurado em Asaas:WebhookToken.
    /// </summary>
    [HttpPost("asaas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Asaas(
        [FromBody] AsaasWebhookEvento evento,
        CancellationToken cancellationToken)
    {
        var tokenEsperado = _config["Asaas:WebhookToken"];
        if (!string.IsNullOrWhiteSpace(tokenEsperado))
        {
            var tokenRecebido = Request.Headers["asaas-access-token"].FirstOrDefault();
            if (tokenRecebido != tokenEsperado)
                return Unauthorized();
        }
        if (evento.Payment is null)
            return Ok();

        var eventosConfirmacao = new[] { "PAYMENT_RECEIVED", "PAYMENT_CONFIRMED" };
        if (!eventosConfirmacao.Contains(evento.Event))
            return Ok();

        var pagamento = evento.Payment;

        if (string.IsNullOrWhiteSpace(pagamento.Id))
            return Ok();

        DateOnly dataPagamento;
        if (!DateOnly.TryParse(pagamento.PaymentDate, out dataPagamento))
            dataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);

        await _mediator.Send(new ConfirmarPagamentoOnlineCommand(
            pagamento.Id,
            pagamento.Value,
            pagamento.BillingType,
            dataPagamento), cancellationToken);

        return Ok();
    }
}
