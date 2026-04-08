using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Despesas.Queries.ListarDespesas;

public class ListarDespesasQueryHandler : IRequestHandler<ListarDespesasQuery, ListaDespesasDto>
{
    private readonly IDespesaReadRepository _repo;

    public ListarDespesasQueryHandler(IDespesaReadRepository repo) => _repo = repo;

    public async Task<ListaDespesasDto> Handle(ListarDespesasQuery request, CancellationToken cancellationToken)
        => await _repo.ListarAsync(
            request.FilialId,
            request.Categoria,
            request.Status,
            request.DataInicio,
            request.DataFim,
            request.Pagina,
            request.TamanhoPagina,
            cancellationToken);
}
