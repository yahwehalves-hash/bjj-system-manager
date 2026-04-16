using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Planos.Commands.CriarPlano;

public class CriarPlanoCommandHandler : IRequestHandler<CriarPlanoCommand, Guid>
{
    private readonly IPlanoRepository _planoRepo;

    public CriarPlanoCommandHandler(IPlanoRepository planoRepo) => _planoRepo = planoRepo;

    public async Task<Guid> Handle(CriarPlanoCommand request, CancellationToken cancellationToken)
    {
        var plano = new Plano(
            request.FilialId,
            request.Nome,
            request.Descricao,
            request.Valor,
            request.Periodicidade);

        await _planoRepo.AdicionarAsync(plano, cancellationToken);
        await _planoRepo.SalvarAlteracoesAsync(cancellationToken);

        return plano.Id;
    }
}
