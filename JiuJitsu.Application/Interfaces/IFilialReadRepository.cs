using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IFilialReadRepository
{
    Task<FilialDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FilialResumoDto>> ListarAsync(bool? ativo, CancellationToken cancellationToken = default);
}
