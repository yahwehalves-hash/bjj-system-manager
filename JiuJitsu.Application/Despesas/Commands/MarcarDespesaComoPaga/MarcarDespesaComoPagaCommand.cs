using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Despesas.Commands.MarcarDespesaComoPaga;

public record MarcarDespesaComoPagaCommand(
    Guid          DespesaId,
    DateOnly      DataPagamento,
    FormaPagamento FormaPagamento,
    string?       Observacao) : IRequest;
