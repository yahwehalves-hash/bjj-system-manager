using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Commands.GerarMensalidades;

public class GerarMensalidadesCommandHandler : IRequestHandler<GerarMensalidadesCommand, int>
{
    private readonly IAtletaReadRepository       _atletaRead;
    private readonly IConfiguracaoReadRepository _configuracaoRead;
    private readonly IMensalidadeRepository      _mensalidadeRepo;
    private readonly IMatriculaRepository        _matriculaRepo;

    public GerarMensalidadesCommandHandler(
        IAtletaReadRepository atletaRead,
        IConfiguracaoReadRepository configuracaoRead,
        IMensalidadeRepository mensalidadeRepo,
        IMatriculaRepository matriculaRepo)
    {
        _atletaRead      = atletaRead;
        _configuracaoRead = configuracaoRead;
        _mensalidadeRepo = mensalidadeRepo;
        _matriculaRepo   = matriculaRepo;
    }

    public async Task<int> Handle(GerarMensalidadesCommand request, CancellationToken cancellationToken)
    {
        // Primeiro dia do mês de competência (ignora o dia informado)
        var competencia = new DateOnly(request.Competencia.Year, request.Competencia.Month, 1);

        // Busca todos os atletas ativos com suas filiais
        var atletas = await _atletaRead.ListarAsync(null, null, 1, int.MaxValue, cancellationToken);

        var mensalidades = new List<Mensalidade>();

        foreach (var atleta in atletas.Itens)
        {
            // Evita duplicatas
            if (await _mensalidadeRepo.ExisteParaAtletaNoMesAsync(atleta.Id, competencia, cancellationToken))
                continue;

            // Busca configuração efetiva da filial do atleta
            var config = await _configuracaoRead.ObterEfetivaAsync(atleta.FilialId, cancellationToken);

            var dataVencimento = new DateOnly(competencia.Year, competencia.Month, config.DiaVencimento);

            // Usa o valor do plano do atleta se houver matrícula ativa
            var matricula = await _matriculaRepo.ObterAtivaDoAtletaAsync(atleta.Id, cancellationToken);
            var valor = matricula is not null
                ? matricula.ValorEfetivo(matricula.Plano?.Valor ?? config.ValorMensalidadePadrao)
                : config.ValorMensalidadePadrao;

            mensalidades.Add(new Mensalidade(
                atleta.Id,
                atleta.FilialId,
                competencia,
                valor,
                dataVencimento));
        }

        if (mensalidades.Count > 0)
        {
            await _mensalidadeRepo.AdicionarVariasAsync(mensalidades, cancellationToken);
            await _mensalidadeRepo.SalvarAlteracoesAsync(cancellationToken);
        }

        return mensalidades.Count;
    }
}
