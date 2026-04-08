using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.RegistrarPagamento;

public record RegistrarPagamentoCommand(
    Guid          MensalidadeId,
    decimal       ValorPago,
    DateOnly      DataPagamento,
    FormaPagamento FormaPagamento,
    string?       Observacao) : IRequest;
