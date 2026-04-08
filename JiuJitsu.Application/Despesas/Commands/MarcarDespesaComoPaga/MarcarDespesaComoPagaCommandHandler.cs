using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Despesas.Commands.MarcarDespesaComoPaga;

public class MarcarDespesaComoPagaCommandHandler : IRequestHandler<MarcarDespesaComoPagaCommand>
{
    private readonly IDespesaRepository _repo;

    public MarcarDespesaComoPagaCommandHandler(IDespesaRepository repo) => _repo = repo;

    public async Task Handle(MarcarDespesaComoPagaCommand request, CancellationToken cancellationToken)
    {
        var despesa = await _repo.ObterPorIdAsync(request.DespesaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Despesa '{request.DespesaId}' não encontrada.");

        despesa.MarcarComoPaga(request.DataPagamento, request.FormaPagamento, request.Observacao);

        await _repo.AtualizarAsync(despesa, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
