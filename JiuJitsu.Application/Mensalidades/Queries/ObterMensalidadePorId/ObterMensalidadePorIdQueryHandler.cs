using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Queries.ObterMensalidadePorId;

public class ObterMensalidadePorIdQueryHandler : IRequestHandler<ObterMensalidadePorIdQuery, MensalidadeDetalheDto?>
{
    private readonly IMensalidadeReadRepository _repo;

    public ObterMensalidadePorIdQueryHandler(IMensalidadeReadRepository repo) => _repo = repo;

    public async Task<MensalidadeDetalheDto?> Handle(ObterMensalidadePorIdQuery request, CancellationToken cancellationToken)
        => await _repo.ObterPorIdAsync(request.Id, cancellationToken);
}
