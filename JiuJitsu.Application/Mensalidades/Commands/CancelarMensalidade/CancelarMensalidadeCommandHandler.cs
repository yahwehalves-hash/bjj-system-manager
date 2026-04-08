using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.CancelarMensalidade;

public class CancelarMensalidadeCommandHandler : IRequestHandler<CancelarMensalidadeCommand>
{
    private readonly IMensalidadeRepository _repo;

    public CancelarMensalidadeCommandHandler(IMensalidadeRepository repo) => _repo = repo;

    public async Task Handle(CancelarMensalidadeCommand request, CancellationToken cancellationToken)
    {
        var mensalidade = await _repo.ObterPorIdAsync(request.MensalidadeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Mensalidade '{request.MensalidadeId}' não encontrada.");

        mensalidade.Cancelar(request.Motivo);

        await _repo.AtualizarAsync(mensalidade, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
