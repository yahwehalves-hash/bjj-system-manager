using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Turmas.Queries.ListarTurmas;

public record ListarTurmasQuery(Guid? FilialId, bool? Ativo) : IRequest<ListaTurmasDto>;
