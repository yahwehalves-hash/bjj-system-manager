using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Queries.ObterHistoricoAtleta;

public record ObterHistoricoAtletaQuery(Guid AtletaId) : IRequest<HistoricoAtletaDto?>;
