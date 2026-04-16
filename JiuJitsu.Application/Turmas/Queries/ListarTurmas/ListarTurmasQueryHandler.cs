using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Turmas.Queries.ListarTurmas;

public class ListarTurmasQueryHandler : IRequestHandler<ListarTurmasQuery, ListaTurmasDto>
{
    private readonly ITurmaReadRepository _turmaRead;

    public ListarTurmasQueryHandler(ITurmaReadRepository turmaRead) => _turmaRead = turmaRead;

    public Task<ListaTurmasDto> Handle(ListarTurmasQuery request, CancellationToken cancellationToken)
        => _turmaRead.ListarAsync(request.FilialId, request.Ativo, cancellationToken);
}
