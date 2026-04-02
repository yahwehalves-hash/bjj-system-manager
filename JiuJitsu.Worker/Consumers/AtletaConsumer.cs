using System.Text;
using System.Text.Json;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Infrastructure.Messaging.Constantes;
using JiuJitsu.Worker.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JiuJitsu.Worker.Consumers;

// BackgroundService que fica escutando a fila do RabbitMQ continuamente
public class AtletaConsumer : BackgroundService
{
    private readonly IConnection _conexao;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AtletaConsumer> _logger;
    private IModel? _canal;

    public AtletaConsumer(
        IConnection conexao,
        IServiceScopeFactory scopeFactory,
        ILogger<AtletaConsumer> logger)
    {
        _conexao      = conexao;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _canal = _conexao.CreateModel();

        // Configura DLX e fila principal (espelha a configuração do publisher)
        _canal.ExchangeDeclare(RabbitMqConstantes.ExchangeDlx, ExchangeType.Fanout, durable: true);
        _canal.QueueDeclare(RabbitMqConstantes.FilaDlq, durable: true, exclusive: false, autoDelete: false);
        _canal.QueueBind(RabbitMqConstantes.FilaDlq, RabbitMqConstantes.ExchangeDlx, string.Empty);

        _canal.ExchangeDeclare(RabbitMqConstantes.Exchange, ExchangeType.Direct, durable: true);
        _canal.QueueDeclare(
            queue: RabbitMqConstantes.Fila,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMqConstantes.ExchangeDlx } });

        foreach (var routingKey in new[] { RabbitMqConstantes.RoutingCriacao, RabbitMqConstantes.RoutingAtualizacao, RabbitMqConstantes.RoutingExclusao })
            _canal.QueueBind(RabbitMqConstantes.Fila, RabbitMqConstantes.Exchange, routingKey);

        // Processa uma mensagem por vez (prefetch = 1) para garantir ordem
        _canal.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(_canal);
        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            AtletaMensagem? mensagem = null;

            try
            {
                mensagem = JsonSerializer.Deserialize<AtletaMensagem>(json);
                if (mensagem is null) throw new InvalidOperationException("Mensagem desserializada é nula.");

                await ProcessarMensagemAsync(mensagem, stoppingToken);
                _canal.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem. Operação: {Op}", mensagem?.Operacao ?? "desconhecida");
                // Rejeita sem requeue — a mensagem vai para a DLQ
                _canal.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _canal.BasicConsume(RabbitMqConstantes.Fila, autoAck: false, consumer);

        _logger.LogInformation("AtletaConsumer iniciado. Aguardando mensagens na fila '{Fila}'", RabbitMqConstantes.Fila);

        // Mantém o BackgroundService ativo até o CancellationToken ser cancelado
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessarMensagemAsync(AtletaMensagem mensagem, CancellationToken cancellationToken)
    {
        // Cria um escopo de DI para cada mensagem (DbContext é Scoped)
        using var escopo = _scopeFactory.CreateScope();

        switch (mensagem.Operacao)
        {
            case "Criacao":
                var handlerCriacao = escopo.ServiceProvider.GetRequiredService<CriarAtletaHandler>();
                await handlerCriacao.ProcessarAsync(mensagem, cancellationToken);
                break;

            case "Atualizacao":
                var handlerAtualizacao = escopo.ServiceProvider.GetRequiredService<AtualizarAtletaHandler>();
                await handlerAtualizacao.ProcessarAsync(mensagem, cancellationToken);
                break;

            case "Exclusao":
                var handlerExclusao = escopo.ServiceProvider.GetRequiredService<ExcluirAtletaHandler>();
                await handlerExclusao.ProcessarAsync(mensagem, cancellationToken);
                break;

            default:
                _logger.LogWarning("Operação desconhecida recebida: {Operacao}", mensagem.Operacao);
                break;
        }
    }

    public override void Dispose()
    {
        _canal?.Dispose();
        base.Dispose();
    }
}
