using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Despesas.Commands.LancarDespesa;

public class LancarDespesaCommandHandler : IRequestHandler<LancarDespesaCommand, Guid>
{
    private readonly IDespesaRepository  _repo;
    private readonly IFilialRepository   _filialRepo;

    public LancarDespesaCommandHandler(IDespesaRepository repo, IFilialRepository filialRepo)
    {
        _repo       = repo;
        _filialRepo = filialRepo;
    }

    public async Task<Guid> Handle(LancarDespesaCommand request, CancellationToken cancellationToken)
    {
        if (!await _filialRepo.ExisteAsync(request.FilialId, cancellationToken))
            throw new KeyNotFoundException($"Filial '{request.FilialId}' não encontrada.");

        var despesa = new Despesa(
            request.FilialId,
            request.Descricao,
            request.Categoria,
            request.Subcategoria,
            request.Valor,
            request.DataCompetencia,
            request.DataPagamento,
            request.FormaPagamento,
            request.Observacao,
            request.RegistradoPorId);

        await _repo.AdicionarAsync(despesa, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);

        return despesa.Id;
    }
}
