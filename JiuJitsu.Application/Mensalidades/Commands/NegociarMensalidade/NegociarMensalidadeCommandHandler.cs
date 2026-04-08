using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.NegociarMensalidade;

public class NegociarMensalidadeCommandHandler : IRequestHandler<NegociarMensalidadeCommand>
{
    private readonly IMensalidadeRepository _repo;

    public NegociarMensalidadeCommandHandler(IMensalidadeRepository repo) => _repo = repo;

    public async Task Handle(NegociarMensalidadeCommand request, CancellationToken cancellationToken)
    {
        var mensalidade = await _repo.ObterPorIdAsync(request.MensalidadeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Mensalidade '{request.MensalidadeId}' não encontrada.");

        mensalidade.Negociar(request.NovoValor, request.NovaDataVencimento, request.Observacao);

        await _repo.AtualizarAsync(mensalidade, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
