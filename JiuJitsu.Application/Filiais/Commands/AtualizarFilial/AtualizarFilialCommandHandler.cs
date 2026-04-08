using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Filiais.Commands.AtualizarFilial;

public class AtualizarFilialCommandHandler : IRequestHandler<AtualizarFilialCommand>
{
    private readonly IFilialRepository _repo;

    public AtualizarFilialCommandHandler(IFilialRepository repo) => _repo = repo;

    public async Task Handle(AtualizarFilialCommand request, CancellationToken cancellationToken)
    {
        var filial = await _repo.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Filial '{request.Id}' não encontrada.");

        filial.Atualizar(request.Nome, request.Endereco, request.Cnpj, request.Telefone);

        await _repo.AtualizarAsync(filial, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
