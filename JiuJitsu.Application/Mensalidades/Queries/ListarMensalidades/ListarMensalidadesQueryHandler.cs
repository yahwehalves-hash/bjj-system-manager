using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Mensalidades.Queries.ListarMensalidades;

public class ListarMensalidadesQueryHandler : IRequestHandler<ListarMensalidadesQuery, ListaMensalidadesDto>
{
    private readonly IMensalidadeReadRepository _repo;

    public ListarMensalidadesQueryHandler(IMensalidadeReadRepository repo) => _repo = repo;

    public async Task<ListaMensalidadesDto> Handle(ListarMensalidadesQuery request, CancellationToken cancellationToken)
        => await _repo.ListarAsync(
            request.FilialId,
            request.AtletaId,
            request.Status,
            request.Competencia,
            request.Pagina,
            request.TamanhoPagina,
            cancellationToken);
}
