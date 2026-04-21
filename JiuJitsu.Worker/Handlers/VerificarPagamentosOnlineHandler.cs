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

        _logger.LogInformation("Polling: {Total} cobrança(s) pendente(s) para verificar.", lista.Count);

        if (lista.Count == 0) return;

        var confirmadas = 0;
        foreach (var mensalidade in lista)
        {
            try
            {
                // Consulta por externalReference (ID da mensalidade) — resiliente a cobranças duplicadas.
                // Encontra qualquer cobrança paga vinculada à mensalidade, independente do ID armazenado.
                var resultado = await _gateway.BuscarPagamentoConfirmadoPorReferenciaAsync(
                    mensalidade.Id.ToString(), cancellationToken);

                if (resultado is null) continue;

                await _mediator.Send(new ConfirmarPagamentoOnlineCommand(
                    resultado.Value.CobrancaId,
                    mensalidade.Valor,
                    resultado.Value.Status,
                    DateOnly.FromDateTime(DateTime.UtcNow)), cancellationToken);

                confirmadas++;
                _logger.LogInformation(
                    "Mensalidade {Id} confirmada via polling. CobrancaId={CobrancaId} Status={Status}",
                    mensalidade.Id, resultado.Value.CobrancaId, resultado.Value.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar mensalidade {MensalidadeId}", mensalidade.Id);
            }
        }

        if (confirmadas > 0)
            _logger.LogInformation("Polling: {Confirmadas} pagamento(s) confirmado(s).", confirmadas);
    }
}
