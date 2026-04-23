using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Pagamento.Commands.ConfirmarPagamentoOnline;
using JiuJitsu.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Application.Pagamento.Commands.VerificarPagamentosGateway;

public class VerificarPagamentosGatewayCommandHandler
    : IRequestHandler<VerificarPagamentosGatewayCommand, int>
{
    private readonly IMensalidadeRepository _repo;
    private readonly IGatewayPagamento      _gateway;
    private readonly IMediator              _mediator;
    private readonly ILogger<VerificarPagamentosGatewayCommandHandler> _logger;

    public VerificarPagamentosGatewayCommandHandler(
        IMensalidadeRepository repo,
        IGatewayPagamento      gateway,
        IMediator              mediator,
        ILogger<VerificarPagamentosGatewayCommandHandler> logger)
    {
        _repo     = repo;
        _gateway  = gateway;
        _mediator = mediator;
        _logger   = logger;
    }

    public async Task<int> Handle(
        VerificarPagamentosGatewayCommand request,
        CancellationToken cancellationToken)
    {
        if (!_gateway.Configurado)
        {
            _logger.LogWarning("Gateway de pagamento não configurado — sincronização ignorada.");
            return 0;
        }

        const int lote = 200;
        var pendentes = await _repo.ListarComCobrancaOnlinePendentesAsync(lote, cancellationToken);
        var lista     = pendentes.ToList();

        _logger.LogInformation(
            "Sincronização manual: verificando {Total} cobrança(s) pendente(s).", lista.Count);

        var confirmadas = 0;

        foreach (var mensalidade in lista)
        {
            try
            {
                // Consulta por externalReference — resiliente a cobranças duplicadas
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
                    "Mensalidade {Id} confirmada via sincronização manual. CobrancaId={CobrancaId} Status={Status}",
                    mensalidade.Id, resultado.Value.CobrancaId, resultado.Value.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao verificar mensalidade {MensalidadeId} durante sincronização manual.",
                    mensalidade.Id);
            }
        }

        _logger.LogInformation(
            "Sincronização manual concluída: {Confirmadas}/{Total} pagamento(s) confirmado(s).",
            confirmadas, lista.Count);

        return confirmadas;
    }
}
