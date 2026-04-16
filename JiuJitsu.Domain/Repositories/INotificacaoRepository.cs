using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface INotificacaoRepository
{
    Task<IEnumerable<TemplateNotificacao>> ListarTemplatesAsync(CancellationToken cancellationToken = default);
    Task<TemplateNotificacao?> ObterTemplatePorEventoECanalAsync(string evento, string canal, CancellationToken cancellationToken = default);
    Task AdicionarTemplateAsync(TemplateNotificacao template, CancellationToken cancellationToken = default);
    Task<TemplateNotificacao?> ObterTemplatePorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoverTemplateAsync(Guid id, CancellationToken cancellationToken = default);
    Task RegistrarHistoricoAsync(HistoricoNotificacao historico, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
