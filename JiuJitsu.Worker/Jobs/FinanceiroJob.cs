using JiuJitsu.Worker.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Jobs;

/// <summary>
/// Job agendado que executa tarefas financeiras periódicas:
/// - PRD: No 1º dia do mês gera mensalidades; diariamente atualiza status vencidas
/// - DEV: Quando Worker:IntervaloSegundos está configurado, roda em loop no intervalo
///        definido e gera mensalidades a cada ciclo (sem restrição de dia 1)
/// </summary>
public class FinanceiroJob : BackgroundService
{
    private readonly IServiceScopeFactory   _scopeFactory;
    private readonly ILogger<FinanceiroJob> _logger;
    private readonly int                    _intervaloSegundos;

    public FinanceiroJob(IServiceScopeFactory scopeFactory, ILogger<FinanceiroJob> logger, IConfiguration config)
    {
        _scopeFactory      = scopeFactory;
        _logger            = logger;
        _intervaloSegundos = config.GetValue<int>("Worker:IntervaloSegundos");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var modoTeste = _intervaloSegundos > 0;

        if (modoTeste)
            _logger.LogInformation("FinanceiroJob iniciado em MODO TESTE — intervalo: {Intervalo}s.", _intervaloSegundos);
        else
            _logger.LogInformation("FinanceiroJob iniciado em MODO PRODUÇÃO — executa diariamente às 06:00 UTC.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan espera;

            if (modoTeste)
            {
                espera = TimeSpan.FromSeconds(_intervaloSegundos);
                _logger.LogInformation("Próxima execução do FinanceiroJob em {Intervalo}s (modo teste).", _intervaloSegundos);
            }
            else
            {
                var agora          = DateTime.UtcNow;
                var proximaExecucao = agora.Date.AddDays(1).AddHours(6);
                espera             = proximaExecucao - agora;
                _logger.LogInformation("Próxima execução do FinanceiroJob em {ProximaExecucao:dd/MM/yyyy HH:mm} UTC.", proximaExecucao);
            }

            await Task.Delay(espera, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            await ExecutarTarefasAsync(modoTeste, stoppingToken);
        }
    }

    private async Task ExecutarTarefasAsync(bool modoTeste, CancellationToken cancellationToken)
    {
        using var escopo = _scopeFactory.CreateScope();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        // Em modo teste gera sempre; em produção apenas no dia 1
        var deveGerar = modoTeste || hoje.Day == 1;

        if (deveGerar)
        {
            try
            {
                var handler = escopo.ServiceProvider.GetRequiredService<GerarMensalidadesHandler>();
                await handler.ProcessarAsync(hoje, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar mensalidades para {Competencia}", hoje);
            }
        }

        // Sempre atualiza status de vencidas/inadimplentes
        try
        {
            var handler = escopo.ServiceProvider.GetRequiredService<AtualizarStatusMensalidadesHandler>();
            await handler.ProcessarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status de mensalidades vencidas");
        }
    }
}
