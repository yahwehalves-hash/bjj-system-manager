using JiuJitsu.Application.Interfaces;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.AtualizarStatusVencidas;

public class AtualizarStatusVencidasCommandHandler : IRequestHandler<AtualizarStatusVencidasCommand, int>
{
    private readonly IMensalidadeRepository      _repo;
    private readonly IConfiguracaoReadRepository _configuracaoRead;

    public AtualizarStatusVencidasCommandHandler(
        IMensalidadeRepository repo,
        IConfiguracaoReadRepository configuracaoRead)
    {
        _repo             = repo;
        _configuracaoRead = configuracaoRead;
    }

    public async Task<int> Handle(AtualizarStatusVencidasCommand request, CancellationToken cancellationToken)
    {
        // Usa a configuração global como referência para o job — cada filial pode ter tolerância diferente,
        // mas por simplicidade o job usa a global. Filiais com override precisariam de lógica por filial.
        var configGlobal = await _configuracaoRead.ObterGlobalAsync(cancellationToken);
        if (configGlobal is null) return 0;

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var atualizadas = 0;

        // Pendentes → Vencidas (após tolerância)
        var pendentesVencidas = await _repo.ListarPendentesVencidasAsync(
            hoje, configGlobal.ToleranciaInadimplenciaDias, cancellationToken);

        foreach (var m in pendentesVencidas)
        {
            m.MarcarComoVencida();
            await _repo.AtualizarAsync(m, cancellationToken);
            atualizadas++;
        }

        // Vencidas → Inadimplentes (após 30 dias de vencida, por exemplo)
        var diasParaInadimplencia = configGlobal.ToleranciaInadimplenciaDias + 30;
        var vencidasInadimplentes = await _repo.ListarVencidasParaInadimplenciaAsync(
            hoje, diasParaInadimplencia, cancellationToken);

        foreach (var m in vencidasInadimplentes)
        {
            m.MarcarComoInadimplente();
            await _repo.AtualizarAsync(m, cancellationToken);
            atualizadas++;
        }

        if (atualizadas > 0)
            await _repo.SalvarAlteracoesAsync(cancellationToken);

        return atualizadas;
    }
}
