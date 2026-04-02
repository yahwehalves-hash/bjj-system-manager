using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Queries.ListarAtletas;

// Filtros opcionais — quando nulos, não são aplicados na consulta
public record ListarAtletasQuery(
    string? Nome,
    string? Faixa,
    int     Pagina       = 1,
    int     TamanhoPagina = 10
) : IRequest<ListaAtletasDto>;
