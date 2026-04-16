using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.VincularAtleta;

public class VincularAtletaTurmaCommandHandler : IRequestHandler<VincularAtletaTurmaCommand>
{
    private readonly ITurmaRepository _turmaRepo;

    public VincularAtletaTurmaCommandHandler(ITurmaRepository turmaRepo) => _turmaRepo = turmaRepo;

    public async Task Handle(VincularAtletaTurmaCommand request, CancellationToken cancellationToken)
    {
        var turma = await _turmaRepo.ObterPorIdAsync(request.TurmaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Turma {request.TurmaId} não encontrada.");

        var jaVinculado = await _turmaRepo.AtletaJaVinculadoAsync(request.AtletaId, request.TurmaId, cancellationToken);
        if (jaVinculado)
            throw new InvalidOperationException("Atleta já está vinculado a esta turma.");

        var atletaTurma = new AtletaTurma(request.AtletaId, request.TurmaId);
        await _turmaRepo.AdicionarAtletaAsync(atletaTurma, cancellationToken);
        await _turmaRepo.SalvarAlteracoesAsync(cancellationToken);
    }
}
