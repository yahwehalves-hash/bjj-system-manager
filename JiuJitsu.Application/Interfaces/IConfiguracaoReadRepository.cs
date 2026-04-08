using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IConfiguracaoReadRepository
{
    Task<ConfiguracaoGlobalDto?> ObterGlobalAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracaoFilialDto?> ObterPorFilialAsync(Guid filialId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna os parâmetros efetivos da filial: valor local se existir, senão valor global (via COALESCE no SQL).
    /// </summary>
    Task<ConfiguracaoEfetivaDto> ObterEfetivaAsync(Guid filialId, CancellationToken cancellationToken = default);
}
