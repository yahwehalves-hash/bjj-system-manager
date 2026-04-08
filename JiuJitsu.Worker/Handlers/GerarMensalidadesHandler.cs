using JiuJitsu.Application.Mensalidades.Commands.GerarMensalidades;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

/// <summary>
/// Handler executado no 1º dia do mês para gerar as mensalidades de todos os atletas ativos.
/// </summary>
public class GerarMensalidadesHandler
{
    private readonly IMediator                    _mediator;
    private readonly ILogger<GerarMensalidadesHandler> _logger;

    public GerarMensalidadesHandler(
        IMediator mediator,
        ILogger<GerarMensalidadesHandler> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    public async Task ProcessarAsync(DateOnly competencia, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gerando mensalidades para competência {Competencia}...", competencia);

        var total = await _mediator.Send(new GerarMensalidadesCommand(competencia), cancellationToken);

        _logger.LogInformation("{Total} mensalidades geradas para {Competencia}.", total, competencia);
    }
}
