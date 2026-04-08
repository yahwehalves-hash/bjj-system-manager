using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Configuracoes.Commands.AtualizarConfiguracaoGlobal;

public class AtualizarConfiguracaoGlobalCommandHandler : IRequestHandler<AtualizarConfiguracaoGlobalCommand>
{
    private readonly IConfiguracaoRepository _repo;

    public AtualizarConfiguracaoGlobalCommandHandler(IConfiguracaoRepository repo) => _repo = repo;

    public async Task Handle(AtualizarConfiguracaoGlobalCommand request, CancellationToken cancellationToken)
    {
        var configuracao = await _repo.ObterGlobalAsync(cancellationToken);

        if (configuracao is null)
        {
            // Primeira vez — cria o registro singleton
            configuracao = new ConfiguracaoGlobal(
                request.ValorMensalidadePadrao,
                request.DiaVencimento,
                request.ToleranciaInadimplenciaDias,
                request.MultaAtrasoPercentual,
                request.JurosDiarioPercentual,
                request.DescontoAntecipacaoPercentual);
            await _repo.AdicionarGlobalAsync(configuracao, cancellationToken);
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
            await _repo.AtualizarGlobalAsync(configuracao, cancellationToken);
        }

        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
