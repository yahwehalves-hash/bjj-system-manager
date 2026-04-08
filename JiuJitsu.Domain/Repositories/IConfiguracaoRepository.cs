using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IConfiguracaoRepository
{
    Task<ConfiguracaoGlobal?> ObterGlobalAsync(CancellationToken cancellationToken = default);
    Task AdicionarGlobalAsync(ConfiguracaoGlobal configuracao, CancellationToken cancellationToken = default);
    Task AtualizarGlobalAsync(ConfiguracaoGlobal configuracao, CancellationToken cancellationToken = default);

    Task<ConfiguracaoFilial?> ObterPorFilialAsync(Guid filialId, CancellationToken cancellationToken = default);
    Task AdicionarFilialAsync(ConfiguracaoFilial configuracao, CancellationToken cancellationToken = default);
    Task AtualizarFilialAsync(ConfiguracaoFilial configuracao, CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
