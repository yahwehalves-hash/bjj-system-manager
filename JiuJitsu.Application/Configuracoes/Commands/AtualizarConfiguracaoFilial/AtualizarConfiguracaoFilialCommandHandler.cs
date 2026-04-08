using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Configuracoes.Commands.AtualizarConfiguracaoFilial;

public class AtualizarConfiguracaoFilialCommandHandler : IRequestHandler<AtualizarConfiguracaoFilialCommand>
{
    private readonly IConfiguracaoRepository _repo;
    private readonly IFilialRepository       _filialRepo;

    public AtualizarConfiguracaoFilialCommandHandler(
        IConfiguracaoRepository repo,
        IFilialRepository filialRepo)
    {
        _repo       = repo;
        _filialRepo = filialRepo;
    }

    public async Task Handle(AtualizarConfiguracaoFilialCommand request, CancellationToken cancellationToken)
    {
        if (!await _filialRepo.ExisteAsync(request.FilialId, cancellationToken))
            throw new KeyNotFoundException($"Filial '{request.FilialId}' não encontrada.");

        var configuracao = await _repo.ObterPorFilialAsync(request.FilialId, cancellationToken);

        if (configuracao is null)
        {
            configuracao = new ConfiguracaoFilial(request.FilialId);
            configuracao.Atualizar(
                request.ValorMensalidadePadrao,
                request.DiaVencimento,
                request.ToleranciaInadimplenciaDias,
                request.MultaAtrasoPercentual,
                request.JurosDiarioPercentual,
                request.DescontoAntecipacaoPercentual,
                request.UsuarioId);
            await _repo.AdicionarFilialAsync(configuracao, cancellationToken);
        }
        else
        {
            configuracao.Atualizar(
                request.ValorMensalidadePadrao,
                request.DiaVencimento,
                request.ToleranciaInadimplenciaDias,
                request.MultaAtrasoPercentual,
                request.JurosDiarioPercentual,
                request.DescontoAntecipacaoPercentual,
                request.UsuarioId);
            await _repo.AtualizarFilialAsync(configuracao, cancellationToken);
        }

        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
