using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IMatriculaRepository
{
    Task<Matricula?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Matricula?> ObterAtivaDoAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Matricula>> ListarPorAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Matricula matricula, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
