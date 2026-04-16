using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Graduacao.Queries.ListarRegras;

public record ListarRegrasGraduacaoQuery(Guid? FilialId) : IRequest<IEnumerable<RegraGraduacaoDto>>;
