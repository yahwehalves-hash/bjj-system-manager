using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Pagamento.Commands.CriarCobrancaOnline;
using JiuJitsu.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

public class GerarCobrancasOnlineHandler
{
    private readonly IMensalidadeRepository _mensalidadeRepo;
    private readonly IGatewayPagamento      _gateway;
    private readonly IMediator              _mediator;
    private readonly ILogger<GerarCobrancasOnlineHandler> _logger;

    public GerarCobrancasOnlineHandler(
        IMensalidadeRepository mensalidadeRepo,
        IGatewayPagamento      gateway,
        IMediator              mediator,
        ILogger<GerarCobrancasOnlineHandler> logger)
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
            _logger.LogDebug("Gateway de pagamento não configurado — job ignorado.");
            return;
        }

        const int lote = 50;
        var pendentes = await _mensalidadeRepo.ListarSemCobrancaOnlineAsync(lote, cancellationToken);
        var lista     = pendentes.ToList();

        _logger.LogInformation("Gerando cobranças online para {Total} mensalidades.", lista.Count);

        var sucesso = 0;
        var falha   = 0;

        foreach (var mensalidade in lista)
        {
            try
            {
                await _mediator.Send(new CriarCobrancaOnlineCommand(mensalidade.Id), cancellationToken);
                sucesso++;
            }
            catch (Exception ex)
            {
                falha++;
                _logger.LogError(ex, "Falha ao criar cobrança para mensalidade {Id}", mensalidade.Id);
            }
        }

        _logger.LogInformation("Cobranças online: {Sucesso} criadas, {Falha} falhas.", sucesso, falha);
    }
}
