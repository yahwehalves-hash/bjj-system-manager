using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Turmas.Queries.ObterTurmaPorId;

public class ObterTurmaPorIdQueryHandler : IRequestHandler<ObterTurmaPorIdQuery, TurmaDetalheDto?>
{
    private readonly ITurmaReadRepository _turmaRead;

    public ObterTurmaPorIdQueryHandler(ITurmaReadRepository turmaRead) => _turmaRead = turmaRead;

    public Task<TurmaDetalheDto?> Handle(ObterTurmaPorIdQuery request, CancellationToken cancellationToken)
        => _turmaRead.ObterPorIdAsync(request.Id, cancellationToken);
}
