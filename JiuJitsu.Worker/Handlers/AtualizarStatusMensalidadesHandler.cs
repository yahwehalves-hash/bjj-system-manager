using JiuJitsu.Application.Mensalidades.Commands.AtualizarStatusVencidas;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

/// <summary>
/// Handler executado pelo job diário para atualizar o status de mensalidades vencidas e inadimplentes.
/// </summary>
public class AtualizarStatusMensalidadesHandler
{
    private readonly IMediator                           _mediator;
    private readonly ILogger<AtualizarStatusMensalidadesHandler> _logger;

    public AtualizarStatusMensalidadesHandler(
        IMediator mediator,
        ILogger<AtualizarStatusMensalidadesHandler> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    public async Task ProcessarAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando atualização de status de mensalidades vencidas...");

        var atualizadas = await _mediator.Send(new AtualizarStatusVencidasCommand(), cancellationToken);

        _logger.LogInformation("Atualização concluída. {Total} mensalidades atualizadas.", atualizadas);
    }
}
