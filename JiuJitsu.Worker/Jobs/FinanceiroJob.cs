using JiuJitsu.Worker.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Jobs;

/// <summary>
/// Job agendado que executa tarefas financeiras periódicas:
/// - No 1º dia do mês: gera mensalidades de todos os atletas ativos
/// - Diariamente: atualiza status de mensalidades vencidas e inadimplentes
/// </summary>
public class FinanceiroJob : BackgroundService
{
    private readonly IServiceScopeFactory         _scopeFactory;
    private readonly ILogger<FinanceiroJob>       _logger;

    public FinanceiroJob(IServiceScopeFactory scopeFactory, ILogger<FinanceiroJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FinanceiroJob iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var agora = DateTime.UtcNow;

            // Aguarda até 06h00 do próximo dia
            var proximaExecucao = agora.Date.AddDays(1).AddHours(6);
            var espera = proximaExecucao - agora;

            _logger.LogInformation("Próxima execução do FinanceiroJob em {ProximaExecucao:dd/MM/yyyy HH:mm} UTC", proximaExecucao);

            await Task.Delay(espera, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            await ExecutarTarefasAsync(stoppingToken);
        }
    }

    private async Task ExecutarTarefasAsync(CancellationToken cancellationToken)
    {
        using var escopo = _scopeFactory.CreateScope();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        // 1º dia do mês → gera mensalidades
        if (hoje.Day == 1)
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

        // Todo dia → atualiza status de vencidas/inadimplentes
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
