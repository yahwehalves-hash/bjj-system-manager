using MediatR;

namespace JiuJitsu.Application.Pagamento.Commands.ConfirmarPagamentoOnline;

/// <summary>
/// Confirma pagamento recebido via webhook ou polling do gateway de pagamento.
/// </summary>
public record ConfirmarPagamentoOnlineCommand(
    string   CobrancaExternaId,
    decimal  ValorPago,
    string   FormaPagamentoGateway,
    DateOnly DataPagamento) : IRequest;
