using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

/// <summary>
/// Dispara notificações de aniversário por e-mail para todos os atletas ativos
/// cujo dia e mês de nascimento coincidem com a data de hoje.
/// </summary>
public class DispararAniversariosHandler
{
    private readonly AppDbContext                          _db;
    private readonly INotificacaoService                  _notificacaoService;
    private readonly ILogger<DispararAniversariosHandler> _logger;

    public DispararAniversariosHandler(
        AppDbContext                          db,
        INotificacaoService                  notificacaoService,
        ILogger<DispararAniversariosHandler> logger)
    {
        _db                 = db;
        _notificacaoService = notificacaoService;
        _logger             = logger;
    }

    public async Task ProcessarAsync(CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);

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

        foreach (var atleta in aniversariantes)
        {
            try
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

                await _notificacaoService.EnviarAsync(mensagem, cancellationToken);

                _logger.LogInformation("Notificação de aniversário enviada para {Atleta} ({Email}).",
                    atleta.NomeCompleto, atleta.Email.Valor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar aniversário do atleta {AtletaId}.", atleta.Id);
            }
        }
    }
}
