using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Graduacao.Queries.ListarElegiveis;

public record ListarElegiveisGraduacaoQuery(Guid? FilialId) : IRequest<IEnumerable<ElegívelGraduacaoDto>>;
