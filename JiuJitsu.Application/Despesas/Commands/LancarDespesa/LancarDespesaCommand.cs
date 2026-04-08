using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Despesas.Commands.LancarDespesa;

public record LancarDespesaCommand(
    Guid            FilialId,
    string          Descricao,
    CategoriaDespesa Categoria,
    string          Subcategoria,
    decimal         Valor,
    DateOnly        DataCompetencia,
    DateOnly?       DataPagamento,
    FormaPagamento? FormaPagamento,
    string?         Observacao,
    Guid?           RegistradoPorId) : IRequest<Guid>;
