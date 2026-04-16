using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Graduacao.Commands.SalvarRegraGraduacao;

public class SalvarRegraGraduacaoCommandHandler : IRequestHandler<SalvarRegraGraduacaoCommand>
{
    private readonly IRegraGraduacaoRepository _repo;

    public SalvarRegraGraduacaoCommandHandler(IRegraGraduacaoRepository repo) => _repo = repo;

    public async Task Handle(SalvarRegraGraduacaoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _repo.ObterAsync(request.FilialId, request.Faixa, request.Grau, cancellationToken);

        if (existente is not null)
            existente.AtualizarTempo(request.TempoMinimoMeses);
        else
        {
            var regra = new RegraGraduacao(request.FilialId, request.Faixa, request.Grau, request.TempoMinimoMeses);
            await _repo.AdicionarOuAtualizarAsync(regra, cancellationToken);
        }

        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
