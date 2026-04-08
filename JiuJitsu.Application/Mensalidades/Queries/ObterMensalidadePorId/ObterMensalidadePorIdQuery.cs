using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Queries.ObterMensalidadePorId;

public record ObterMensalidadePorIdQuery(Guid Id) : IRequest<MensalidadeDetalheDto?>;
