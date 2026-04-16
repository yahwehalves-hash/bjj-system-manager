using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class NotificacaoRepository : INotificacaoRepository
{
    private readonly AppDbContext _db;

    public NotificacaoRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<TemplateNotificacao>> ListarTemplatesAsync(CancellationToken cancellationToken = default)
        => await _db.TemplatesNotificacao.ToListAsync(cancellationToken);

    public Task<TemplateNotificacao?> ObterTemplatePorEventoECanalAsync(
        string evento, string canal, CancellationToken cancellationToken = default)
        => _db.TemplatesNotificacao
            .FirstOrDefaultAsync(t => t.Evento == evento && t.Canal == canal && t.Ativo, cancellationToken);

    public async Task AdicionarTemplateAsync(TemplateNotificacao template, CancellationToken cancellationToken = default)
        => await _db.TemplatesNotificacao.AddAsync(template, cancellationToken);

    public Task<TemplateNotificacao?> ObterTemplatePorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.TemplatesNotificacao.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task RemoverTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _db.TemplatesNotificacao.FindAsync([id], cancellationToken);
        if (template is not null) _db.TemplatesNotificacao.Remove(template);
    }

    public async Task RegistrarHistoricoAsync(HistoricoNotificacao historico, CancellationToken cancellationToken = default)
        => await _db.HistoricoNotificacoes.AddAsync(historico, cancellationToken);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
