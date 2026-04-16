using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;

namespace JiuJitsu.Domain.Repositories;

public interface IRegraGraduacaoRepository
{
    Task<RegraGraduacao?> ObterAsync(Guid? filialId, Faixa faixa, Grau grau, CancellationToken cancellationToken = default);
    Task<IEnumerable<RegraGraduacao>> ListarAsync(Guid? filialId, CancellationToken cancellationToken = default);
    Task AdicionarOuAtualizarAsync(RegraGraduacao regra, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
