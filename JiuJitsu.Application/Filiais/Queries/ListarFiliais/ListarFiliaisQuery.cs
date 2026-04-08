using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Filiais.Queries.ListarFiliais;

public record ListarFiliaisQuery(bool? Ativo) : IRequest<IEnumerable<FilialResumoDto>>;
