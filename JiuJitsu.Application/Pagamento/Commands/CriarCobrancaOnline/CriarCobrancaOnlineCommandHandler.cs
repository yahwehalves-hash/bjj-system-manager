using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Application.Pagamento.Commands.CriarCobrancaOnline;

public class CriarCobrancaOnlineCommandHandler : IRequestHandler<CriarCobrancaOnlineCommand, CobrancaOnlineDto>
{
    private readonly IMensalidadeRepository _mensalidadeRepo;
    private readonly IAtletaRepository      _atletaRepo;
    private readonly IGatewayPagamento      _gateway;
    private readonly IEmailService          _email;
    private readonly ILogger<CriarCobrancaOnlineCommandHandler> _logger;

    public CriarCobrancaOnlineCommandHandler(
        IMensalidadeRepository mensalidadeRepo,
        IAtletaRepository      atletaRepo,
        IGatewayPagamento      gateway,
        IEmailService          email,
        ILogger<CriarCobrancaOnlineCommandHandler> logger)
    {
        _mensalidadeRepo = mensalidadeRepo;
        _atletaRepo      = atletaRepo;
        _gateway         = gateway;
        _email           = email;
        _logger          = logger;
    }

    public async Task<CobrancaOnlineDto> Handle(
        CriarCobrancaOnlineCommand request, CancellationToken cancellationToken)
    {
        if (!_gateway.Configurado)
            throw new InvalidOperationException("Gateway de pagamento não configurado.");

        var mensalidade = await _mensalidadeRepo.ObterPorIdAsync(request.MensalidadeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Mensalidade '{request.MensalidadeId}' não encontrada.");

        if (!string.IsNullOrWhiteSpace(mensalidade.CobrancaExternaId))
            return new CobrancaOnlineDto(
                mensalidade.Id,
                mensalidade.CobrancaExternaId,
                mensalidade.LinkPagamento,
                mensalidade.PixCopiaCola);

        var atleta = await _atletaRepo.ObterPorIdAsync(mensalidade.AtletaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Atleta '{mensalidade.AtletaId}' não encontrado.");

        // Idempotência: verifica se já existe cobrança no gateway antes de criar outra.
        // Evita duplicatas em caso de race condition entre o job automático e ação manual.
        var existente = await _gateway.BuscarCobrancaExistentePorReferenciaAsync(
            mensalidade.Id.ToString(), cancellationToken);

        if (existente is not null)
        {
            _logger.LogInformation(
                "Cobrança já existente no gateway para mensalidade {MensalidadeId}: {CobrancaId}. Reutilizando.",
                mensalidade.Id, existente.CobrancaId);

            mensalidade.VincularCobrancaOnline(existente.CobrancaId, existente.LinkPagamento, existente.PixCopiaCola);
            await _mensalidadeRepo.AtualizarAsync(mensalidade, cancellationToken);
            await _mensalidadeRepo.SalvarAlteracoesAsync(cancellationToken);

            return new CobrancaOnlineDto(
                mensalidade.Id, existente.CobrancaId, existente.LinkPagamento, existente.PixCopiaCola);
        }

        var clienteId = await _gateway.ObterOuCriarClienteAsync(
            atleta.Cpf.Valor,
            atleta.NomeCompleto,
            atleta.Email.Valor,
            atleta.Telefone,
            cancellationToken);

        var descricao = $"Mensalidade {mensalidade.Competencia:MM/yyyy}";
        var resultado = await _gateway.CriarCobrancaAsync(
            clienteId,
            mensalidade.Valor,
            mensalidade.DataVencimento,
            descricao,
            mensalidade.Id.ToString(),
            cancellationToken);

        mensalidade.VincularCobrancaOnline(resultado.CobrancaId, resultado.LinkPagamento, resultado.PixCopiaCola);
        await _mensalidadeRepo.AtualizarAsync(mensalidade, cancellationToken);
        await _mensalidadeRepo.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Cobrança vinculada à mensalidade {MensalidadeId}: {CobrancaId}",
            mensalidade.Id, resultado.CobrancaId);

        await EnviarEmailCobrancaAsync(atleta.Email.Valor, atleta.NomeCompleto,
            mensalidade.Competencia, mensalidade.Valor,
            resultado.PixCopiaCola, resultado.LinkPagamento,
            cancellationToken);

        return new CobrancaOnlineDto(
            mensalidade.Id,
            resultado.CobrancaId,
            resultado.LinkPagamento,
            resultado.PixCopiaCola);
    }

    private async Task EnviarEmailCobrancaAsync(
        string email, string nomeAtleta, DateOnly competencia, decimal valor,
        string? pixCopiaCola, string? linkPagamento, CancellationToken cancellationToken)
    {
        try
        {
            var corpo = $"""
                Olá, {nomeAtleta}!

                Sua mensalidade de {competencia:MM/yyyy} está disponível para pagamento.

                Valor: R$ {valor:N2}

                """;

            if (!string.IsNullOrWhiteSpace(pixCopiaCola))
                corpo += $"""
                PIX Copia e Cola:
                {pixCopiaCola}

                """;

            if (!string.IsNullOrWhiteSpace(linkPagamento))
                corpo += $"""
                Ou acesse o link de pagamento:
                {linkPagamento}

                """;

            corpo += "Em caso de dúvidas, entre em contato com sua academia.";

            await _email.EnviarAsync(
                email,
                $"Mensalidade {competencia:MM/yyyy} — R$ {valor:N2}",
                corpo,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível enviar email de cobrança para {Email}", email);
        }
    }
}
