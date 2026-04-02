using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Queries.ObterAtletaPorId;

public record ObterAtletaPorIdQuery(Guid Id) : IRequest<AtletaDetalheDto?>;
