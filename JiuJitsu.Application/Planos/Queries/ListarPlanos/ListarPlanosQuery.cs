using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Planos.Queries.ListarPlanos;

public record ListarPlanosQuery(Guid? FilialId) : IRequest<IEnumerable<PlanoDto>>;
