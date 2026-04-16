using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Turmas.Commands.CriarTurma;

public class CriarTurmaCommandHandler : IRequestHandler<CriarTurmaCommand, Guid>
{
    private readonly ITurmaRepository _turmaRepo;

    public CriarTurmaCommandHandler(ITurmaRepository turmaRepo) => _turmaRepo = turmaRepo;

    public async Task<Guid> Handle(CriarTurmaCommand request, CancellationToken cancellationToken)
    {
        var turma = new Turma(
            request.FilialId,
            request.Nome,
            request.ProfessorId,
            request.DiasSemana,
            request.Horario,
            request.CapacidadeMaxima);

        await _turmaRepo.AdicionarAsync(turma, cancellationToken);
        await _turmaRepo.SalvarAlteracoesAsync(cancellationToken);

        return turma.Id;
    }
}
