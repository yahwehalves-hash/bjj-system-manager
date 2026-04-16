using MediatR;

namespace JiuJitsu.Application.Matriculas.Commands.CriarMatricula;

public record CriarMatriculaCommand(
    Guid     AtletaId,
    Guid     PlanoId,
    DateOnly DataInicio,
    DateOnly? DataFim,
    decimal? ValorCustomizado,
    string?  Observacao) : IRequest<Guid>;
