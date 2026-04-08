using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Despesas.Commands.CancelarDespesa;

public class CancelarDespesaCommandHandler : IRequestHandler<CancelarDespesaCommand>
{
    private readonly IDespesaRepository _repo;

    public CancelarDespesaCommandHandler(IDespesaRepository repo) => _repo = repo;

    public async Task Handle(CancelarDespesaCommand request, CancellationToken cancellationToken)
    {
        var despesa = await _repo.ObterPorIdAsync(request.DespesaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Despesa '{request.DespesaId}' não encontrada.");

        despesa.Cancelar(request.Motivo);

        await _repo.AtualizarAsync(despesa, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
