using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Filiais.Queries.ObterFilialPorId;

public class ObterFilialPorIdQueryHandler : IRequestHandler<ObterFilialPorIdQuery, FilialDetalheDto?>
{
    private readonly IFilialReadRepository _repo;

    public ObterFilialPorIdQueryHandler(IFilialReadRepository repo) => _repo = repo;

    public async Task<FilialDetalheDto?> Handle(ObterFilialPorIdQuery request, CancellationToken cancellationToken)
        => await _repo.ObterPorIdAsync(request.Id, cancellationToken);
}
