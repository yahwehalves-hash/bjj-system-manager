using JiuJitsu.Worker.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Jobs;

/// <summary>
/// Job que cria cobranças online para mensalidades pendentes sem link de pagamento.
/// - PRD: Executa 2x ao dia (06:30 e 12:00 UTC)
/// - DEV: Quando Worker:IntervaloSegundos está configurado, roda no intervalo definido
/// </summary>
public class CobrancaOnlineJob : BackgroundService
{
    private readonly IServiceScopeFactory    _scopeFactory;
    private readonly ILogger<CobrancaOnlineJob> _logger;
    private readonly int                     _intervaloSegundos;

    public CobrancaOnlineJob(
        IServiceScopeFactory    scopeFactory,
        ILogger<CobrancaOnlineJob> logger,
        IConfiguration          config)
    {
        _scopeFactory      = scopeFactory;
        _logger            = logger;
        _intervaloSegundos = config.GetValue<int>("Worker:IntervaloSegundos");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var modoTeste = _intervaloSegundos > 0;

        _logger.LogInformation(
            modoTeste
                ? "CobrancaOnlineJob iniciado em MODO TESTE — intervalo: {Intervalo}s."
                : "CobrancaOnlineJob iniciado em MODO PRODUÇÃO — executa 2x ao dia.",
            _intervaloSegundos);

        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

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
                var agora = DateTime.UtcNow;
                var candidatos = new[]
                {
                    agora.Date.AddHours(6.5),
                    agora.Date.AddHours(12),
                    agora.Date.AddDays(1).AddHours(6.5)
                };
                var proxima = candidatos.First(t => t > agora);
                espera = proxima - agora;
                _logger.LogInformation("Próxima execução do CobrancaOnlineJob em {Data:dd/MM/yyyy HH:mm} UTC.", proxima);
            }

            await Task.Delay(espera, stoppingToken);
        }
    }

    private async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        using var escopo = _scopeFactory.CreateScope();
        try
        {
            var handler = escopo.ServiceProvider.GetRequiredService<GerarCobrancasOnlineHandler>();
            await handler.ProcessarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no CobrancaOnlineJob.");
        }
    }
}
