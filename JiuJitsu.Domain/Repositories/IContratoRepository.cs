using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IContratoRepository
{
    Task<TemplateContrato?> ObterTemplateAtivoAsync(Guid? filialId, CancellationToken cancellationToken = default);
    Task AdicionarTemplateAsync(TemplateContrato template, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterContratoAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default);
    Task AdicionarContratoAsync(Contrato contrato, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
