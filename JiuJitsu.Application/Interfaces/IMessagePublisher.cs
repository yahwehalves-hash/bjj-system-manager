using JiuJitsu.Contracts.Mensagens;

namespace JiuJitsu.Application.Interfaces;

// Abstração da mensageria — permite trocar RabbitMQ por outro broker sem alterar a Application
public interface IMessagePublisher
{
    Task PublicarAsync(AtletaMensagem mensagem, CancellationToken cancellationToken = default);
}
