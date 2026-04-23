using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Infrastructure.Notificacoes;

/// <summary>
/// Envia notificações de eventos internos (aniversário, inatividade) por e-mail.
/// Notificações de cobrança e inadimplência são gerenciadas diretamente pelo Asaas.
/// </summary>
public class NotificacaoService : INotificacaoService
{
    private readonly INotificacaoRepository      _repo;
    private readonly IEmailService               _emailService;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(
        INotificacaoRepository      repo,
        IEmailService               emailService,
        ILogger<NotificacaoService> logger)
    {
        _repo         = repo;
        _emailService = emailService;
        _logger       = logger;
    }

    public async Task EnviarAsync(NotificacaoMensagem mensagem, CancellationToken cancellationToken = default)
    {
        var template = await _repo.ObterTemplatePorEventoECanalAsync(mensagem.Evento, "Email", cancellationToken);

        var enviou = false;

        if (template is not null && !string.IsNullOrWhiteSpace(mensagem.Email))
        {
            var texto = InterpolaMensagem(template.Mensagem, mensagem);
            try
            {
                await _emailService.EnviarAsync(
                    mensagem.Email,
                    $"[{mensagem.NomeAcademia}] {mensagem.Evento}",
                    texto,
                    cancellationToken);
                enviou = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar e-mail para atleta {AtletaId}", mensagem.AtletaId);
            }
        }

        var historico = new HistoricoNotificacao(
            mensagem.AtletaId,
            mensagem.Evento,
            enviou ? "Email" : "N/A",
            enviou ? "Enviado" : "Falhou");

        await _repo.RegistrarHistoricoAsync(historico, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }

    private static string InterpolaMensagem(string template, NotificacaoMensagem m) =>
        template
            .Replace("{NomeAtleta}",     m.NomeAtleta)
            .Replace("{NomeAcademia}",   m.NomeAcademia)
            .Replace("{Valor}",          m.Valor?.ToString("C") ?? "")
            .Replace("{DataVencimento}", m.DataVencimento?.ToString("dd/MM/yyyy") ?? "");
}
