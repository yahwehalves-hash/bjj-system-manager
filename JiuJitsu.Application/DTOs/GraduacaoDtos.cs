namespace JiuJitsu.Application.DTOs;

public record RegraGraduacaoDto(
    Guid   Id,
    Guid?  FilialId,
    string Faixa,
    int    Grau,
    int    TempoMinimoMeses);

public record ElegívelGraduacaoDto(
    Guid     AtletaId,
    string   NomeAtleta,
    Guid     FilialId,
    string   NomeFilial,
    string   FaixaAtual,
    int      GrauAtual,
    DateOnly DataUltimaGraduacao,
    int      MesesNaFaixa,
    int      TempoMinimoNecessario);
