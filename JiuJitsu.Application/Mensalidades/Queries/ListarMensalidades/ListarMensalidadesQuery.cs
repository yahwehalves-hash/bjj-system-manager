using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Queries.ListarMensalidades;

public record ListarMensalidadesQuery(
    Guid?   FilialId,
    Guid?   AtletaId,
    string? Status,
    string? Competencia,
    int     Pagina        = 1,
    int     TamanhoPagina = 10) : IRequest<ListaMensalidadesDto>;
