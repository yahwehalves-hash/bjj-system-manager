using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Worker.Handlers;

public class CriarAtletaHandler
{
    private readonly IAtletaRepository _repositorio;
    private readonly IEmailService     _emailService;
    private readonly ILogger<CriarAtletaHandler> _logger;

    public CriarAtletaHandler(
        IAtletaRepository repositorio,
        IEmailService emailService,
        ILogger<CriarAtletaHandler> logger)
    {
        _repositorio  = repositorio;
        _emailService = emailService;
        _logger       = logger;
    }

    public async Task ProcessarAsync(AtletaMensagem mensagem, CancellationToken cancellationToken)
    {
        var payload = mensagem.Payload!;

        // O Worker cria o atleta com um novo ID gerado no construtor.
        // O ID retornado pela API é apenas um correlation ID para rastreamento.
        var atleta = new Atleta(
            nomeCompleto:        payload.NomeCompleto,
            cpf:                 new Cpf(payload.Cpf),
            dataNascimento:      payload.DataNascimento,
            faixa:               Enum.Parse<Faixa>(payload.Faixa),
            grau:                (Grau)payload.Grau,
            dataUltimaGraduacao: payload.DataUltimaGraduacao,
            email:               new Email(payload.Email));

        await _repositorio.AdicionarAsync(atleta, cancellationToken);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Atleta '{Nome}' criado com sucesso (Id: {Id})", payload.NomeCompleto, atleta.Id);

        await _emailService.EnviarAsync(
            destinatario: payload.Email,
            assunto:      "Cadastro realizado com sucesso!",
            corpo:        $"Olá {payload.NomeCompleto}, seu cadastro foi realizado com sucesso!\n\nFaixa: {payload.Faixa} {payload.Grau}° grau",
            cancellationToken);
    }
}
