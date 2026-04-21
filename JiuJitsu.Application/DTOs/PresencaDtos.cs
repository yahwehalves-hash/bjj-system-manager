namespace JiuJitsu.Application.DTOs;

public record RegistroPresencaDto(
    Guid     Id,
    Guid     AtletaId,
    string   NomeAtleta,
    Guid     TurmaId,
    string   NomeTurma,
    Guid     FilialId,
    DateTime DataHora,
    string   Origem);

public record ListaPresencasDto(
    IEnumerable<RegistroPresencaDto> Itens,
    int TotalItens);

public record FrequenciaAtletaDto(
    Guid    AtletaId,
    string  NomeAtleta,
    Guid    TurmaId,
    string  NomeTurma,
    int     TotalPresencas,
    int     TotalAulas,
    decimal PercentualFrequencia,
    DateTime? UltimaPresenca);

public record FrequenciaTurmaDto(
    Guid                            TurmaId,
    string                          NomeTurma,
    DateOnly                        DataInicio,
    DateOnly                        DataFim,
    IEnumerable<FrequenciaAtletaDto> Atletas);

public record AtletaInativoDto(
    Guid     AtletaId,
    string   NomeAtleta,
    string?  Telefone,
    string   Email,
    Guid     FilialId,
    string   NomeFilial,
    DateTime UltimaPresenca,
    int      DiasInativo);
