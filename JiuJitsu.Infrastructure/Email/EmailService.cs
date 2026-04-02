using JiuJitsu.Application.Interfaces;
using JiuJitsu.Infrastructure.Email.Configuracoes;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JiuJitsu.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailConfiguracoes _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailConfiguracoes> config, ILogger<EmailService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task EnviarAsync(
        string destinatario,
        string assunto,
        string corpo,
        CancellationToken cancellationToken = default)
    {
        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(_config.NomeRemetente, _config.Remetente));
        mensagem.To.Add(MailboxAddress.Parse(destinatario));
        mensagem.Subject = assunto;
        mensagem.Body    = new TextPart("plain") { Text = corpo };

        using var cliente = new SmtpClient();

        try
        {
            await cliente.ConnectAsync(_config.ServidorSmtp, _config.Porta, useSsl: false, cancellationToken);
            await cliente.SendAsync(mensagem, cancellationToken);
            await cliente.DisconnectAsync(quit: true, cancellationToken);

            _logger.LogInformation("Email enviado para {Destinatario} com assunto '{Assunto}'", destinatario, assunto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {Destinatario}", destinatario);
            throw;
        }
    }
}
