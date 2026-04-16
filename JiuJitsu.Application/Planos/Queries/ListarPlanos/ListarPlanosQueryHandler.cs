using JiuJitsu.Application.DTOs;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Planos.Queries.ListarPlanos;

public class ListarPlanosQueryHandler : IRequestHandler<ListarPlanosQuery, IEnumerable<PlanoDto>>
{
    private readonly IPlanoRepository _planoRepo;

    public ListarPlanosQueryHandler(IPlanoRepository planoRepo) => _planoRepo = planoRepo;

    public async Task<IEnumerable<PlanoDto>> Handle(ListarPlanosQuery request, CancellationToken cancellationToken)
    {
        var planos = await _planoRepo.ListarAtivosAsync(request.FilialId, cancellationToken);

        return planos.Select(p => new PlanoDto(
            p.Id,
            p.FilialId,
            p.Nome,
            p.Descricao,
            p.Valor,
            p.Periodicidade.ToString(),
            p.Ativo));
    }
}
