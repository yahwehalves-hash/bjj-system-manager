using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Planos.Commands.DesativarPlano;

public class DesativarPlanoCommandHandler : IRequestHandler<DesativarPlanoCommand>
{
    private readonly IPlanoRepository _planoRepo;

    public DesativarPlanoCommandHandler(IPlanoRepository planoRepo) => _planoRepo = planoRepo;

    public async Task Handle(DesativarPlanoCommand request, CancellationToken cancellationToken)
    {
        var plano = await _planoRepo.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Plano {request.Id} não encontrado.");

        plano.Desativar();
        await _planoRepo.SalvarAlteracoesAsync(cancellationToken);
    }
}
