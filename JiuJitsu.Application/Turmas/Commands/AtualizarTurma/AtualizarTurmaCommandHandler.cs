using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.AtualizarTurma;

public class AtualizarTurmaCommandHandler : IRequestHandler<AtualizarTurmaCommand>
{
    private readonly ITurmaRepository _turmaRepo;

    public AtualizarTurmaCommandHandler(ITurmaRepository turmaRepo) => _turmaRepo = turmaRepo;

    public async Task Handle(AtualizarTurmaCommand request, CancellationToken cancellationToken)
    {
        var turma = await _turmaRepo.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Turma {request.Id} não encontrada.");

        turma.Atualizar(request.Nome, request.ProfessorId, request.DiasSemana, request.Horario, request.CapacidadeMaxima);

        await _turmaRepo.AtualizarAsync(turma, cancellationToken);
        await _turmaRepo.SalvarAlteracoesAsync(cancellationToken);
    }
}
