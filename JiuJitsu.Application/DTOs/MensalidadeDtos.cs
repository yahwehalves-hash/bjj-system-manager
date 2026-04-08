namespace JiuJitsu.Application.DTOs;

public record MensalidadeResumoDto(
    Guid     Id,
    Guid     AtletaId,
    string   NomeAtleta,
    Guid     FilialId,
    string   NomeFilial,
    DateOnly Competencia,
    decimal  Valor,
    decimal? ValorPago,
    DateOnly DataVencimento,
    DateOnly? DataPagamento,
    string   Status,
    string?  FormaPagamento);

public record MensalidadeDetalheDto(
    Guid     Id,
    Guid     AtletaId,
    string   NomeAtleta,
    Guid     FilialId,
    string   NomeFilial,
    DateOnly Competencia,
    decimal  Valor,
    decimal? ValorPago,
    DateOnly DataVencimento,
    DateOnly? DataPagamento,
    string   Status,
    string?  FormaPagamento,
    string?  Observacao,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public record ListaMensalidadesDto(
    IEnumerable<MensalidadeResumoDto> Itens,
    int TotalItens,
    int Pagina,
    int TamanhoPagina);
