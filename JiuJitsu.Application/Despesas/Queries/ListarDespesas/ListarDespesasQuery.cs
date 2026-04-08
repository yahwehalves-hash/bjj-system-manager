using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Despesas.Queries.ListarDespesas;

public record ListarDespesasQuery(
    Guid?    FilialId,
    string?  Categoria,
    string?  Status,
    DateOnly? DataInicio,
    DateOnly? DataFim,
    int      Pagina        = 1,
    int      TamanhoPagina = 10) : IRequest<ListaDespesasDto>;
