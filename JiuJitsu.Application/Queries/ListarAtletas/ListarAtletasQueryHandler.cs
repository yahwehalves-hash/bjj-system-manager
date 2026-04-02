using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using MediatR;

namespace JiuJitsu.Application.Queries.ListarAtletas;

public class ListarAtletasQueryHandler : IRequestHandler<ListarAtletasQuery, ListaAtletasDto>
{
    private readonly IAtletaReadRepository _readRepository;

    public ListarAtletasQueryHandler(IAtletaReadRepository readRepository)
        => _readRepository = readRepository;

    public async Task<ListaAtletasDto> Handle(ListarAtletasQuery request, CancellationToken cancellationToken)
        => await _readRepository.ListarAsync(
            request.Nome,
            request.Faixa,
            request.Pagina,
            request.TamanhoPagina,
            cancellationToken);
}
