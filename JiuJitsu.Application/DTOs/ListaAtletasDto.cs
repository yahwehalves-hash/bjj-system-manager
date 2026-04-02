namespace JiuJitsu.Application.DTOs;

// Resultado paginado da listagem de atletas
public record ListaAtletasDto(
    IEnumerable<AtletaResumoDto> Itens,
    int TotalItens,
    int Pagina,
    int TamanhoPagina
);
