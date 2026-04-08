namespace JiuJitsu.Application.DTOs;

public record DespesaResumoDto(
    Guid     Id,
    Guid     FilialId,
    string   NomeFilial,
    string   Descricao,
    string   Categoria,
    string   Subcategoria,
    decimal  Valor,
    DateOnly DataCompetencia,
    DateOnly? DataPagamento,
    string   Status,
    string?  FormaPagamento);

public record DespesaDetalheDto(
    Guid     Id,
    Guid     FilialId,
    string   NomeFilial,
    string   Descricao,
    string   Categoria,
    string   Subcategoria,
    decimal  Valor,
    DateOnly DataCompetencia,
    DateOnly? DataPagamento,
    string   Status,
    string?  FormaPagamento,
    string?  Observacao,
    Guid?    RegistradoPorId,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public record ListaDespesasDto(
    IEnumerable<DespesaResumoDto> Itens,
    int TotalItens,
    int Pagina,
    int TamanhoPagina);
