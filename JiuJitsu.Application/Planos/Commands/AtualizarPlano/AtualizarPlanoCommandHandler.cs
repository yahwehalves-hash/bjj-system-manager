using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Planos.Commands.AtualizarPlano;

public class AtualizarPlanoCommandHandler : IRequestHandler<AtualizarPlanoCommand>
{
    private readonly IPlanoRepository _planoRepo;

    public AtualizarPlanoCommandHandler(IPlanoRepository planoRepo) => _planoRepo = planoRepo;

    public async Task Handle(AtualizarPlanoCommand request, CancellationToken cancellationToken)
    {
        var plano = await _planoRepo.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Plano {request.Id} não encontrado.");

        plano.Atualizar(request.Nome, request.Descricao, request.Valor, request.Periodicidade);
        await _planoRepo.SalvarAlteracoesAsync(cancellationToken);
    }
}
