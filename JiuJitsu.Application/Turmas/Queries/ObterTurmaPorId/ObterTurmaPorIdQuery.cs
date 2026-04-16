using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Turmas.Queries.ObterTurmaPorId;

public record ObterTurmaPorIdQuery(Guid Id) : IRequest<TurmaDetalheDto?>;
