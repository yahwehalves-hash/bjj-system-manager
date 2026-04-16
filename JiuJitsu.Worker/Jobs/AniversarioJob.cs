using JiuJitsu.Worker.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Jobs;

/// <summary>
/// Job agendado que dispara notificações de aniversário para atletas ativos.
/// - PRD: Executa diariamente às 08:00 UTC
/// - DEV: Quando Worker:IntervaloSegundos está configurado, roda no intervalo definido
/// </summary>
public class AniversarioJob : BackgroundService
{
    private readonly IServiceScopeFactory  _scopeFactory;
    private readonly ILogger<AniversarioJob> _logger;
    private readonly int                   _intervaloSegundos;

    public AniversarioJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AniversarioJob> logger,
        IConfiguration config)
    {
        _scopeFactory      = scopeFactory;
        _logger            = logger;
        _intervaloSegundos = config.GetValue<int>("Worker:IntervaloSegundos");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var modoTeste = _intervaloSegundos > 0;

        if (modoTeste)
            _logger.LogInformation("AniversarioJob iniciado em MODO TESTE — intervalo: {Intervalo}s.", _intervaloSegundos);
        else
            _logger.LogInformation("AniversarioJob iniciado em MODO PRODUÇÃO — executa diariamente às 08:00 UTC.");

        // Aguarda um pouco para não executar imediatamente ao subir
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

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
                var agora           = DateTime.UtcNow;
                var proximaExecucao = agora.Date.AddDays(1).AddHours(8);
                espera              = proximaExecucao - agora;
                _logger.LogInformation("Próxima execução do AniversarioJob em {Data:dd/MM/yyyy HH:mm} UTC.", proximaExecucao);
            }

            await Task.Delay(espera, stoppingToken);
        }
    }

    private async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        using var escopo = _scopeFactory.CreateScope();
        try
        {
            var handler = escopo.ServiceProvider.GetRequiredService<DispararAniversariosHandler>();
            await handler.ProcessarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao disparar notificações de aniversário.");
        }
    }
}
