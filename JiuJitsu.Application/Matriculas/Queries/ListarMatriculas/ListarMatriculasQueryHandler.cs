using JiuJitsu.Application.DTOs;
using JiuJitsu.Domain.Repositories;
using MediatR;

namespace JiuJitsu.Application.Matriculas.Queries.ListarMatriculas;

public class ListarMatriculasQueryHandler : IRequestHandler<ListarMatriculasQuery, IEnumerable<MatriculaDto>>
{
    private readonly IMatriculaRepository _matriculaRepo;

    public ListarMatriculasQueryHandler(IMatriculaRepository matriculaRepo)
        => _matriculaRepo = matriculaRepo;

    public async Task<IEnumerable<MatriculaDto>> Handle(ListarMatriculasQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Matricula> matriculas;

        if (request.AtletaId.HasValue)
            matriculas = await _matriculaRepo.ListarPorAtletaAsync(request.AtletaId.Value, cancellationToken);
        else
            matriculas = [];

        return matriculas.Select(m => new MatriculaDto(
            m.Id,
            m.AtletaId,
            m.Atleta?.NomeCompleto ?? string.Empty,
            m.PlanoId,
            m.Plano?.Nome ?? string.Empty,
            m.DataInicio,
            m.DataFim,
            m.ValorCustomizado,
            m.ValorEfetivo(m.Plano?.Valor ?? 0),
            m.Status.ToString(),
            m.Observacao));
    }
}
