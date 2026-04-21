using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Pagamento.Commands.ConfirmarPagamentoOnline;
using JiuJitsu.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

/// <summary>
/// Consulta o gateway de pagamento para verificar se cobranças pendentes foram pagas.
/// Complementa o webhook — garante consistência mesmo se o webhook falhar.
/// </summary>
public class VerificarPagamentosOnlineHandler
{
    private readonly IMensalidadeRepository _mensalidadeRepo;
    private readonly IGatewayPagamento      _gateway;
    private readonly IMediator              _mediator;
    private readonly ILogger<VerificarPagamentosOnlineHandler> _logger;

    private static readonly HashSet<string> _statusPagos =
        new(StringComparer.OrdinalIgnoreCase) { "RECEIVED", "CONFIRMED", "RECEIVED_IN_CASH" };

    public VerificarPagamentosOnlineHandler(
        IMensalidadeRepository mensalidadeRepo,
        IGatewayPagamento      gateway,
        IMediator              mediator,
        ILogger<VerificarPagamentosOnlineHandler> logger)
    {
        _mensalidadeRepo = mensalidadeRepo;
        _gateway         = gateway;
        _mediator        = mediator;
        _logger          = logger;
    }

    public async Task ProcessarAsync(CancellationToken cancellationToken)
    {
        if (!_gateway.Configurado)
        {
            _logger.LogDebug("Gateway de pagamento não configurado — polling ignorado.");
            return;
        }

        const int lote = 100;
        var pendentes = await _mensalidadeRepo.ListarComCobrancaOnlinePendentesAsync(lote, cancellationToken);
        var lista     = pendentes.ToList();

        if (lista.Count == 0)
        {
            _logger.LogDebug("Nenhuma cobrança pendente para verificar.");
            return;
        }

        _logger.LogInformation("Verificando status de {Total} cobranças.", lista.Count);

        var confirmadas = 0;
        foreach (var mensalidade in lista)
        {
            try
            {
                var status = await _gateway.ConsultarStatusCobrancaAsync(mensalidade.CobrancaExternaId!, cancellationToken);

                if (status is null || !_statusPagos.Contains(status))
                    continue;

                await _mediator.Send(new ConfirmarPagamentoOnlineCommand(
                    mensalidade.CobrancaExternaId!,
                    mensalidade.Valor,
                    "PIX",
                    DateOnly.FromDateTime(DateTime.UtcNow)), cancellationToken);

                confirmadas++;
                _logger.LogInformation(
                    "Mensalidade {Id} confirmada via polling. Status: {Status}",
                    mensalidade.Id, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar cobrança {CobrancaId}", mensalidade.CobrancaExternaId);
            }
        }

        if (confirmadas > 0)
            _logger.LogInformation("Polling: {Confirmadas} pagamento(s) confirmado(s).", confirmadas);
    }
}
