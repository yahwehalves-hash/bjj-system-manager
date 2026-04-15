using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IHistoricoGraduacaoRepository
{
    Task AdicionarAsync(HistoricoGraduacao historico, CancellationToken cancellationToken = default);
    Task<HistoricoGraduacao?> ObterAtualAsync(Guid atletaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HistoricoGraduacao>> ListarPorAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
