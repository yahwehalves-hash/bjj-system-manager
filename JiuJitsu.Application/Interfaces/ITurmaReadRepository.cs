using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface ITurmaReadRepository
{
    Task<ListaTurmasDto> ListarAsync(Guid? filialId, bool? ativo, CancellationToken cancellationToken = default);
    Task<TurmaDetalheDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
