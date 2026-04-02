using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Queries.ObterAtletaPorId;

public class ObterAtletaPorIdQueryHandler : IRequestHandler<ObterAtletaPorIdQuery, AtletaDetalheDto?>
{
    private readonly IAtletaReadRepository _readRepository;

    public ObterAtletaPorIdQueryHandler(IAtletaReadRepository readRepository)
        => _readRepository = readRepository;

    public async Task<AtletaDetalheDto?> Handle(ObterAtletaPorIdQuery request, CancellationToken cancellationToken)
        => await _readRepository.ObterPorIdAsync(request.Id, cancellationToken);
}
