using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.DesvincularAtleta;

public class DesvincularAtletaTurmaCommandHandler : IRequestHandler<DesvincularAtletaTurmaCommand>
{
    private readonly ITurmaRepository _turmaRepo;

    public DesvincularAtletaTurmaCommandHandler(ITurmaRepository turmaRepo) => _turmaRepo = turmaRepo;

    public async Task Handle(DesvincularAtletaTurmaCommand request, CancellationToken cancellationToken)
    {
        await _turmaRepo.RemoverAtletaAsync(request.AtletaId, request.TurmaId, cancellationToken);
        await _turmaRepo.SalvarAlteracoesAsync(cancellationToken);
    }
}
