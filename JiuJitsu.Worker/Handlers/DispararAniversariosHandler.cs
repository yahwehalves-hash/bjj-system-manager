using System.Text;
using System.Text.Json;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Infrastructure.Messaging.Constantes;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace JiuJitsu.Worker.Handlers;

/// <summary>
/// Dispara notificações de aniversário para todos os atletas ativos
/// cujo dia e mês de nascimento coincidem com a data de hoje.
/// </summary>
public class DispararAniversariosHandler
{
    private readonly AppDbContext                       _db;
    private readonly IConnection                       _conexao;
    private readonly ILogger<DispararAniversariosHandler> _logger;

    public DispararAniversariosHandler(
        AppDbContext db,
        IConnection conexao,
        ILogger<DispararAniversariosHandler> logger)
    {
        _db      = db;
        _conexao = conexao;
        _logger  = logger;
    }

    public async Task ProcessarAsync(CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var aniversariantes = await _db.Atletas
            .Include(a => a.Filial)
            .Where(a => a.Ativo
                     && a.DataNascimento.Day   == hoje.Day
                     && a.DataNascimento.Month == hoje.Month)
            .ToListAsync(cancellationToken);

        if (aniversariantes.Count == 0)
        {
            _logger.LogInformation("Nenhum aniversariante encontrado para {Data}.", hoje);
            return;
        }

        _logger.LogInformation("{Total} aniversariante(s) encontrado(s) para {Data}.", aniversariantes.Count, hoje);

        using var canal = _conexao.CreateModel();

        canal.ExchangeDeclare(RabbitMqConstantes.NotificacaoExchangeDlx, ExchangeType.Fanout, durable: true);
        canal.QueueDeclare(RabbitMqConstantes.NotificacaoFilaDlq, durable: true, exclusive: false, autoDelete: false);
        canal.QueueBind(RabbitMqConstantes.NotificacaoFilaDlq, RabbitMqConstantes.NotificacaoExchangeDlx, string.Empty);

        canal.ExchangeDeclare(RabbitMqConstantes.NotificacaoExchange, ExchangeType.Direct, durable: true);
        canal.QueueDeclare(
            RabbitMqConstantes.NotificacaoFila, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMqConstantes.NotificacaoExchangeDlx } });
        canal.QueueBind(RabbitMqConstantes.NotificacaoFila, RabbitMqConstantes.NotificacaoExchange, RabbitMqConstantes.RoutingNotificacaoAniversario);

        var props = canal.CreateBasicProperties();
        props.Persistent  = true;
        props.ContentType = "application/json";

        foreach (var atleta in aniversariantes)
        {
            var mensagem = new NotificacaoMensagem
            {
                Evento       = "aniversario.atleta",
                AtletaId     = atleta.Id,
                NomeAtleta   = atleta.NomeCompleto,
                Email        = atleta.Email.Valor,
                Telefone     = atleta.Telefone,
                NomeAcademia = atleta.Filial?.Nome ?? "Academia",
                OcorridoEm   = DateTime.UtcNow,
            };

            var corpo = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensagem));
            canal.BasicPublish(
                exchange:        RabbitMqConstantes.NotificacaoExchange,
                routingKey:      RabbitMqConstantes.RoutingNotificacaoAniversario,
                mandatory:       false,
                basicProperties: props,
                body:            corpo);

            _logger.LogInformation("Notificação de aniversário disparada para {Atleta} ({Email}).",
                atleta.NomeCompleto, atleta.Email.Valor);
        }
    }
}
