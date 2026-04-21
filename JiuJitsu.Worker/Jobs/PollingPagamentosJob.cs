using JiuJitsu.Worker.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Jobs;

/// <summary>
/// Job que consulta o gateway de pagamento periodicamente para confirmar pagamentos.
/// Complementa o webhook — garante consistência mesmo se o gateway não conseguir entregar.
/// - PRD: A cada 4 horas
/// - DEV: Quando Worker:IntervaloSegundos está configurado, roda no intervalo definido
/// </summary>
public class PollingPagamentosJob : BackgroundService
{
    private readonly IServiceScopeFactory    _scopeFactory;
    private readonly ILogger<PollingPagamentosJob> _logger;
    private readonly int                     _intervaloSegundos;

    public PollingPagamentosJob(
        IServiceScopeFactory    scopeFactory,
        ILogger<PollingPagamentosJob> logger,
        IConfiguration          config)
    {
        _scopeFactory      = scopeFactory;
        _logger            = logger;
        _intervaloSegundos = config.GetValue<int?>("Worker:Jobs:PollingPagamentos") ?? -1;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_intervaloSegundos == 0)
        {
            _logger.LogInformation("PollingPagamentosJob desativado via configuração (Worker:Jobs:PollingPagamentos=0).");
            return;
        }

        var modoTeste = _intervaloSegundos > 0;

        _logger.LogInformation(
            modoTeste
                ? "PollingPagamentosJob iniciado em MODO TESTE — intervalo: {Intervalo}s."
                : "PollingPagamentosJob iniciado em MODO PRODUÇÃO — executa a cada 4 horas.",
            _intervaloSegundos);

        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ExecutarAsync(stoppingToken);

            var espera = modoTeste
                ? TimeSpan.FromSeconds(_intervaloSegundos)
                : TimeSpan.FromHours(4);

            await Task.Delay(espera, stoppingToken);
        }
    }

    private async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        using var escopo = _scopeFactory.CreateScope();
        try
        {
            var handler = escopo.ServiceProvider.GetRequiredService<VerificarPagamentosOnlineHandler>();
            await handler.ProcessarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no PollingPagamentosJob.");
        }
    }
}
