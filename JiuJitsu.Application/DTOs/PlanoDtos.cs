namespace JiuJitsu.Application.DTOs;

public record PlanoDto(
    Guid    Id,
    Guid?   FilialId,
    string  Nome,
    string? Descricao,
    decimal Valor,
    string  Periodicidade,
    bool    Ativo);

public record MatriculaDto(
    Guid     Id,
    Guid     AtletaId,
    string   NomeAtleta,
    Guid     PlanoId,
    string   NomePlano,
    DateOnly DataInicio,
    DateOnly? DataFim,
    decimal? ValorCustomizado,
    decimal  ValorEfetivo,
    string   Status,
    string?  Observacao);
