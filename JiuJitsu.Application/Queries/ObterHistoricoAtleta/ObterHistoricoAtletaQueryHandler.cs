using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Queries.ObterHistoricoAtleta;

public class ObterHistoricoAtletaQueryHandler : IRequestHandler<ObterHistoricoAtletaQuery, HistoricoAtletaDto?>
{
    private readonly IHistoricoGraduacaoReadRepository _readRepository;

    public ObterHistoricoAtletaQueryHandler(IHistoricoGraduacaoReadRepository readRepository)
        => _readRepository = readRepository;

    public async Task<HistoricoAtletaDto?> Handle(ObterHistoricoAtletaQuery request, CancellationToken cancellationToken)
        => await _readRepository.ObterHistoricoAtletaAsync(request.AtletaId, cancellationToken);
}
