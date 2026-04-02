namespace JiuJitsu.Application.Interfaces;

// Abstração do serviço de email — implementada com MailKit na Infrastructure
public interface IEmailService
{
    Task EnviarAsync(
        string destinatario,
        string assunto,
        string corpo,
        CancellationToken cancellationToken = default);
}
