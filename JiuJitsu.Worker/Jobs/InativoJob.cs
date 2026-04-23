using JiuJitsu.Worker.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Jobs;

/// <summary>
/// Job que verifica atletas inativos e dispara notificações via WhatsApp/Email.
/// - PRD: Executa diariamente às 09:00 UTC
/// - DEV: Quando Worker:IntervaloSegundos está configurado, roda no intervalo definido
/// </summary>
public class InativoJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InativoJob>  _logger;
    private readonly int                  _intervaloSegundos;

    public InativoJob(
        IServiceScopeFactory scopeFactory,
        ILogger<InativoJob> logger,
        IConfiguration config)
    {
        _scopeFactory      = scopeFactory;
        _logger            = logger;
        _intervaloSegundos = config.GetValue<int?>("Worker:Jobs:Inativo") ?? -1;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_intervaloSegundos == 0)
        {
            _logger.LogInformation("InativoJob desativado via configuração (Worker:Jobs:Inativo=0).");
            return;
        }

        var modoTeste = _intervaloSegundos > 0;

        _logger.LogInformation(
            modoTeste
                ? "InativoJob iniciado em MODO TESTE — intervalo: {Intervalo}s."
                : "InativoJob iniciado em MODO PRODUÇÃO — executa diariamente às 09:00 UTC.",
            _intervaloSegundos);

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ExecutarAsync(stoppingToken);

            TimeSpan espera;
            if (modoTeste)
            {
                espera = TimeSpan.FromSeconds(_intervaloSegundos);
            }
            else
            {
                var agora          = DateTime.UtcNow;
                var proximaExecucao = agora.Date.AddDays(1).AddHours(9);
                espera             = proximaExecucao - agora;
                _logger.LogInformation("Próxima execução do InativoJob em {Data:dd/MM/yyyy HH:mm} UTC.", proximaExecucao);
            }

            await Task.Delay(espera, stoppingToken);
        }
    }

    private async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        using var escopo = _scopeFactory.CreateScope();
        try
        {
            var handler = escopo.ServiceProvider.GetRequiredService<VerificarInativosHandler>();
            await handler.ProcessarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar atletas inativos.");
        }
    }
}
