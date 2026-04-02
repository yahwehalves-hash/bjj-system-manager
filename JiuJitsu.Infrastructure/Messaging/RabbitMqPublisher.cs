using System.Text;
using System.Text.Json;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Infrastructure.Messaging.Constantes;
using RabbitMQ.Client;

namespace JiuJitsu.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IConnection _conexao;

    public RabbitMqPublisher(IConnection conexao) => _conexao = conexao;

    public Task PublicarAsync(AtletaMensagem mensagem, CancellationToken cancellationToken = default)
    {
        using var canal = _conexao.CreateModel();

        // Declara a Dead Letter Exchange para capturar mensagens com falha
        canal.ExchangeDeclare(
            exchange:   RabbitMqConstantes.ExchangeDlx,
            type:       ExchangeType.Fanout,
            durable:    true,
            autoDelete: false);

        canal.QueueDeclare(
            queue:      RabbitMqConstantes.FilaDlq,
            durable:    true,
            exclusive:  false,
            autoDelete: false);

        canal.QueueBind(
            queue:      RabbitMqConstantes.FilaDlq,
            exchange:   RabbitMqConstantes.ExchangeDlx,
            routingKey: string.Empty);

        // Declara a exchange principal
        canal.ExchangeDeclare(
            exchange:   RabbitMqConstantes.Exchange,
            type:       ExchangeType.Direct,
            durable:    true,
            autoDelete: false);

        // Declara a fila principal com DLX configurada
        canal.QueueDeclare(
            queue:      RabbitMqConstantes.Fila,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments: new Dictionary<string, object> {
                { "x-dead-letter-exchange", RabbitMqConstantes.ExchangeDlx }
            });

        // Vincula a fila às routing keys de cada operação
        foreach (var routingKey in new[] {
            RabbitMqConstantes.RoutingCriacao,
            RabbitMqConstantes.RoutingAtualizacao,
            RabbitMqConstantes.RoutingExclusao })
        {
            canal.QueueBind(
                queue:      RabbitMqConstantes.Fila,
                exchange:   RabbitMqConstantes.Exchange,
                routingKey: routingKey);
        }

        var routingKeyMensagem = mensagem.Operacao switch
        {
            "Criacao"     => RabbitMqConstantes.RoutingCriacao,
            "Atualizacao" => RabbitMqConstantes.RoutingAtualizacao,
            "Exclusao"    => RabbitMqConstantes.RoutingExclusao,
            _             => throw new ArgumentException($"Operação inválida: {mensagem.Operacao}")
        };

        var json = JsonSerializer.Serialize(mensagem);
        var corpo = Encoding.UTF8.GetBytes(json);

        var propriedades = canal.CreateBasicProperties();
        propriedades.Persistent  = true;
        propriedades.ContentType = "application/json";

        canal.BasicPublish(
            exchange:         RabbitMqConstantes.Exchange,
            routingKey:       routingKeyMensagem,
            mandatory:        false,
            basicProperties:  propriedades,
            body:             corpo);

        return Task.CompletedTask;
    }
}
