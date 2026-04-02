using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

public class ExcluirAtletaHandler
{
    private readonly IAtletaRepository _repositorio;
    private readonly IEmailService     _emailService;
    private readonly ILogger<ExcluirAtletaHandler> _logger;

    public ExcluirAtletaHandler(
        IAtletaRepository repositorio,
        IEmailService emailService,
        ILogger<ExcluirAtletaHandler> logger)
    {
        _repositorio  = repositorio;
        _emailService = emailService;
        _logger       = logger;
    }

    public async Task ProcessarAsync(AtletaMensagem mensagem, CancellationToken cancellationToken)
    {
        var atleta = await _repositorio.ObterPorIdAsync(mensagem.AtletaId, cancellationToken);
        if (atleta is null)
        {
            _logger.LogWarning("Atleta {Id} não encontrado para exclusão", mensagem.AtletaId);
            return;
        }

        atleta.Desativar();

        await _repositorio.AtualizarAsync(atleta, cancellationToken);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Atleta {Id} desativado (soft delete)", mensagem.AtletaId);
    }
}
