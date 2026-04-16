using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.DesativarTurma;

public class DesativarTurmaCommandHandler : IRequestHandler<DesativarTurmaCommand>
{
    private readonly ITurmaRepository _turmaRepo;

    public DesativarTurmaCommandHandler(ITurmaRepository turmaRepo) => _turmaRepo = turmaRepo;

    public async Task Handle(DesativarTurmaCommand request, CancellationToken cancellationToken)
    {
        var turma = await _turmaRepo.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Turma {request.Id} não encontrada.");

        turma.Desativar();
        await _turmaRepo.SalvarAlteracoesAsync(cancellationToken);
    }
}
