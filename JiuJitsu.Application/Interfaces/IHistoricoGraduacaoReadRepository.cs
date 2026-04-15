using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IHistoricoGraduacaoReadRepository
{
    Task<HistoricoAtletaDto?> ObterHistoricoAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default);
}
