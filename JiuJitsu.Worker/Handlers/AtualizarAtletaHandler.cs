using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

public class AtualizarAtletaHandler
{
    private readonly IAtletaRepository              _repositorio;
    private readonly IHistoricoGraduacaoRepository  _historicoRepositorio;
    private readonly IEmailService                  _emailService;
    private readonly ILogger<AtualizarAtletaHandler> _logger;

    public AtualizarAtletaHandler(
        IAtletaRepository              repositorio,
        IHistoricoGraduacaoRepository  historicoRepositorio,
        IEmailService                  emailService,
        ILogger<AtualizarAtletaHandler> logger)
    {
        _repositorio          = repositorio;
        _historicoRepositorio = historicoRepositorio;
        _emailService         = emailService;
        _logger               = logger;
    }

    public async Task ProcessarAsync(AtletaMensagem mensagem, CancellationToken cancellationToken)
    {
        var payload = mensagem.Payload!;

        var atleta = await _repositorio.ObterPorIdAsync(mensagem.AtletaId, cancellationToken);
        if (atleta is null)
        {
            _logger.LogWarning("Atleta {Id} não encontrado para atualização", mensagem.AtletaId);
            return;
        }

        var novaFaixa = Enum.Parse<Faixa>(payload.Faixa);
        var novoGrau  = (Grau)payload.Grau;
        var graduacaoMudou = atleta.Faixa != novaFaixa || atleta.Grau != novoGrau;

        atleta.Atualizar(
            nomeCompleto:        payload.NomeCompleto,
            dataNascimento:      payload.DataNascimento,
            faixa:               novaFaixa,
            grau:                novoGrau,
            dataUltimaGraduacao: payload.DataUltimaGraduacao,
            email:               new Email(payload.Email),
            telefone:            payload.Telefone);

        if (graduacaoMudou)
        {
            var historicoAtual = await _historicoRepositorio.ObterAtualAsync(atleta.Id, cancellationToken);
            historicoAtual?.Fechar(payload.DataUltimaGraduacao.AddDays(-1));

            var novoHistorico = new HistoricoGraduacao(
                atletaId:   atleta.Id,
                faixa:      novaFaixa,
                grau:       novoGrau,
                dataInicio: payload.DataUltimaGraduacao);

            await _historicoRepositorio.AdicionarAsync(novoHistorico, cancellationToken);
        }

        await _repositorio.AtualizarAsync(atleta, cancellationToken);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Atleta {Id} atualizado com sucesso", mensagem.AtletaId);

        await _emailService.EnviarAsync(
            destinatario: payload.Email,
            assunto:      "Cadastro atualizado",
            corpo:        $"Olá {payload.NomeCompleto}, seus dados foram atualizados com sucesso.",
            cancellationToken);
    }
}
