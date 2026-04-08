using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Despesas.Queries.ObterDespesaPorId;

public record ObterDespesaPorIdQuery(Guid Id) : IRequest<DespesaDetalheDto?>;
