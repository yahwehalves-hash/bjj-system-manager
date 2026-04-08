using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Configuracoes.Queries.ObterConfiguracaoEfetiva;

public class ObterConfiguracaoEfetivaQueryHandler : IRequestHandler<ObterConfiguracaoEfetivaQuery, ConfiguracaoEfetivaDto>
{
    private readonly IConfiguracaoReadRepository _repo;

    public ObterConfiguracaoEfetivaQueryHandler(IConfiguracaoReadRepository repo) => _repo = repo;

    public async Task<ConfiguracaoEfetivaDto> Handle(ObterConfiguracaoEfetivaQuery request, CancellationToken cancellationToken)
        => await _repo.ObterEfetivaAsync(request.FilialId, cancellationToken);
}
