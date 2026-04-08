using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Filiais.Queries.ObterFilialPorId;

public record ObterFilialPorIdQuery(Guid Id) : IRequest<FilialDetalheDto?>;
