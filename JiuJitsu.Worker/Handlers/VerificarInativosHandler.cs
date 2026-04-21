using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

public class VerificarInativosHandler
{
    private readonly IPresencaReadRepository _presencaRead;
    private readonly INotificacaoService     _notificacaoService;
    private readonly IConfiguration          _config;
    private readonly ILogger<VerificarInativosHandler> _logger;

    public VerificarInativosHandler(
        IPresencaReadRepository    presencaRead,
        INotificacaoService        notificacaoService,
        IConfiguration             config,
        ILogger<VerificarInativosHandler> logger)
    {
        _presencaRead       = presencaRead;
        _notificacaoService = notificacaoService;
        _config             = config;
        _logger             = logger;
    }

    public async Task ProcessarAsync(CancellationToken cancellationToken)
    {
        var diasInativo = _config.GetValue<int>("Presenca:DiasInativoAlerta", 14);

        var inativos = await _presencaRead.ListarInativosAsync(null, diasInativo, cancellationToken);
        var lista    = inativos.ToList();

        _logger.LogInformation("Atletas inativos há mais de {Dias} dias: {Total}", diasInativo, lista.Count);

        foreach (var atleta in lista)
        {
            try
            {
                var mensagem = new NotificacaoMensagem
                {
                    AtletaId     = atleta.AtletaId,
                    NomeAtleta   = atleta.NomeAtleta,
                    Telefone     = atleta.Telefone,
                    Email        = atleta.Email,
                    NomeAcademia = atleta.NomeFilial,
                    Evento       = "Inatividade"
                };

                await _notificacaoService.EnviarAsync(mensagem, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar atleta inativo {AtletaId}", atleta.AtletaId);
            }
        }
    }
}
