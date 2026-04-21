using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IRegistroPresencaRepository
{
    Task AdicionarAsync(RegistroPresenca registro, CancellationToken cancellationToken = default);
    Task AdicionarEmLoteAsync(IEnumerable<RegistroPresenca> registros, CancellationToken cancellationToken = default);
    Task<bool> JaRegistradoHojeAsync(Guid atletaId, Guid turmaId, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
