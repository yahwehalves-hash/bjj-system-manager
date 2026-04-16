using JiuJitsu.Contracts.Mensagens;

namespace JiuJitsu.Application.Interfaces;

public interface INotificacaoService
{
    Task EnviarAsync(NotificacaoMensagem mensagem, CancellationToken cancellationToken = default);
    Task PublicarCobrancaAsync(NotificacaoMensagem mensagem, CancellationToken cancellationToken = default);
}
