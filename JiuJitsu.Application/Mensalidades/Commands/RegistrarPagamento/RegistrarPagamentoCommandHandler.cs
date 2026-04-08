using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.RegistrarPagamento;

public class RegistrarPagamentoCommandHandler : IRequestHandler<RegistrarPagamentoCommand>
{
    private readonly IMensalidadeRepository _repo;

    public RegistrarPagamentoCommandHandler(IMensalidadeRepository repo) => _repo = repo;

    public async Task Handle(RegistrarPagamentoCommand request, CancellationToken cancellationToken)
    {
        var mensalidade = await _repo.ObterPorIdAsync(request.MensalidadeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Mensalidade '{request.MensalidadeId}' não encontrada.");

        mensalidade.RegistrarPagamento(
            request.ValorPago,
            request.DataPagamento,
            request.FormaPagamento,
            request.Observacao);

        await _repo.AtualizarAsync(mensalidade, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
