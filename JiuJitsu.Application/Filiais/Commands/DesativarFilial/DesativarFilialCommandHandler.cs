using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Filiais.Commands.DesativarFilial;

public class DesativarFilialCommandHandler : IRequestHandler<DesativarFilialCommand>
{
    private readonly IFilialRepository _repo;

    public DesativarFilialCommandHandler(IFilialRepository repo) => _repo = repo;

    public async Task Handle(DesativarFilialCommand request, CancellationToken cancellationToken)
    {
        var filial = await _repo.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Filial '{request.Id}' não encontrada.");

        filial.Desativar();
        await _repo.AtualizarAsync(filial, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
