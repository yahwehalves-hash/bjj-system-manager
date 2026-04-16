using System.Text;
using System.Text.Json;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Infrastructure.Messaging.Constantes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JiuJitsu.Worker.Consumers;

public class NotificacaoConsumer : BackgroundService
{
    private readonly IConnection            _conexao;
    private readonly IServiceScopeFactory   _scopeFactory;
    private readonly ILogger<NotificacaoConsumer> _logger;
    private IModel? _canal;

    public NotificacaoConsumer(
        IConnection conexao,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificacaoConsumer> logger)
    {
        _conexao      = conexao;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _canal = _conexao.CreateModel();

        _canal.ExchangeDeclare(RabbitMqConstantes.NotificacaoExchangeDlx, ExchangeType.Fanout, durable: true);
        _canal.QueueDeclare(RabbitMqConstantes.NotificacaoFilaDlq, durable: true, exclusive: false, autoDelete: false);
        _canal.QueueBind(RabbitMqConstantes.NotificacaoFilaDlq, RabbitMqConstantes.NotificacaoExchangeDlx, string.Empty);

        _canal.ExchangeDeclare(RabbitMqConstantes.NotificacaoExchange, ExchangeType.Direct, durable: true);
        _canal.QueueDeclare(
            RabbitMqConstantes.NotificacaoFila, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMqConstantes.NotificacaoExchangeDlx } });

        foreach (var rk in new[] { RabbitMqConstantes.RoutingNotificacaoCobranca, RabbitMqConstantes.RoutingNotificacaoAniversario })
            _canal.QueueBind(RabbitMqConstantes.NotificacaoFila, RabbitMqConstantes.NotificacaoExchange, rk);

        _canal.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_canal);
        consumer.Received += async (_, ea) =>
        {
            var json     = Encoding.UTF8.GetString(ea.Body.ToArray());
            var mensagem = JsonSerializer.Deserialize<NotificacaoMensagem>(json);

            if (mensagem is null)
            {
                _canal.BasicNack(ea.DeliveryTag, false, false);
                return;
            }

            try
            {
                await using var escopo   = _scopeFactory.CreateAsyncScope();
                var notificacaoService   = escopo.ServiceProvider.GetRequiredService<INotificacaoService>();
                await notificacaoService.EnviarAsync(mensagem, stoppingToken);
                _canal.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar notificação para atleta {AtletaId}", mensagem.AtletaId);
                _canal.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _canal.BasicConsume(RabbitMqConstantes.NotificacaoFila, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _canal?.Close();
        base.Dispose();
    }
}
