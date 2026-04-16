using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IGraduacaoReadRepository
{
    Task<IEnumerable<RegraGraduacaoDto>> ListarRegrasAsync(Guid? filialId, CancellationToken ct = default);
    Task<IEnumerable<ElegívelGraduacaoDto>> ListarElegiveisAsync(Guid? filialId, CancellationToken ct = default);
}
