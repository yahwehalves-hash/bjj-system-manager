using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JiuJitsu.Application.Pagamento.Commands.ConfirmarPagamentoOnline;

public class ConfirmarPagamentoOnlineCommandHandler : IRequestHandler<ConfirmarPagamentoOnlineCommand>
{
    private readonly IMensalidadeRepository _repo;
    private readonly ILogger<ConfirmarPagamentoOnlineCommandHandler> _logger;

    public ConfirmarPagamentoOnlineCommandHandler(
        IMensalidadeRepository repo,
        ILogger<ConfirmarPagamentoOnlineCommandHandler> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task Handle(ConfirmarPagamentoOnlineCommand request, CancellationToken cancellationToken)
    {
        var mensalidade = await _repo.ObterPorCobrancaExternaIdAsync(request.CobrancaExternaId, cancellationToken);

        if (mensalidade is null)
        {
            _logger.LogWarning("Cobrança {CobrancaId} não encontrada.", request.CobrancaExternaId);
            return;
        }

        if (mensalidade.Status == StatusMensalidade.Paga)
        {
            _logger.LogInformation("Mensalidade {Id} já paga, ignorando.", mensalidade.Id);
            return;
        }

        var forma = MapearFormaPagamento(request.FormaPagamentoGateway);

        mensalidade.RegistrarPagamento(
            request.ValorPago,
            request.DataPagamento,
            forma,
            $"Pagamento confirmado via gateway ({request.CobrancaExternaId})");

        await _repo.AtualizarAsync(mensalidade, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation(
            "Mensalidade {Id} paga via gateway. Forma: {Forma}, Valor: {Valor}",
            mensalidade.Id, forma, request.ValorPago);
    }

    private static FormaPagamento MapearFormaPagamento(string billingType) => billingType switch
    {
        "PIX"         => FormaPagamento.Pix,
        "BOLETO"      => FormaPagamento.Boleto,
        "CREDIT_CARD" => FormaPagamento.Cartao,
        "DEBIT_CARD"  => FormaPagamento.Cartao,
        _             => FormaPagamento.Pix
    };
}
