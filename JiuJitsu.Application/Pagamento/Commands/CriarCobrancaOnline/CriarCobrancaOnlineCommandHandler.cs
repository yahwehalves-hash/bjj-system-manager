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
    private readonly ILogger<CriarCobrancaOnlineCommandHandler> _logger;

    public CriarCobrancaOnlineCommandHandler(
        IMensalidadeRepository mensalidadeRepo,
        IAtletaRepository      atletaRepo,
        IGatewayPagamento      gateway,
        ILogger<CriarCobrancaOnlineCommandHandler> logger)
    {
        _mensalidadeRepo = mensalidadeRepo;
        _atletaRepo      = atletaRepo;
        _gateway         = gateway;
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
            atleta.CanalNotificacao,
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

        // Notificação ao aluno é gerenciada pelo Asaas (e-mail de cobrança criada + lembretes de inadimplência)

        return new CobrancaOnlineDto(
            mensalidade.Id,
            resultado.CobrancaId,
            resultado.LinkPagamento,
            resultado.PixCopiaCola);
    }
}
