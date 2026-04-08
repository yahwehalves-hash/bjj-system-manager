using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Configuracoes.Queries.ObterConfiguracaoGlobal;

public class ObterConfiguracaoGlobalQueryHandler : IRequestHandler<ObterConfiguracaoGlobalQuery, ConfiguracaoGlobalDto?>
{
    private readonly IConfiguracaoReadRepository _repo;

    public ObterConfiguracaoGlobalQueryHandler(IConfiguracaoReadRepository repo) => _repo = repo;

    public async Task<ConfiguracaoGlobalDto?> Handle(ObterConfiguracaoGlobalQuery request, CancellationToken cancellationToken)
        => await _repo.ObterGlobalAsync(cancellationToken);
}
