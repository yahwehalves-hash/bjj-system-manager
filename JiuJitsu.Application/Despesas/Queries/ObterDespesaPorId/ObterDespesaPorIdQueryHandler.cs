using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Despesas.Queries.ObterDespesaPorId;

public class ObterDespesaPorIdQueryHandler : IRequestHandler<ObterDespesaPorIdQuery, DespesaDetalheDto?>
{
    private readonly IDespesaReadRepository _repo;

    public ObterDespesaPorIdQueryHandler(IDespesaReadRepository repo) => _repo = repo;

    public async Task<DespesaDetalheDto?> Handle(ObterDespesaPorIdQuery request, CancellationToken cancellationToken)
        => await _repo.ObterPorIdAsync(request.Id, cancellationToken);
}
