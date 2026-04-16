using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Commands.AtualizarAtleta;

public record AtualizarAtletaCommand(
    Guid     Id,
    string   NomeCompleto,
    DateOnly DataNascimento,
    Faixa    Faixa,
    Grau     Grau,
    DateOnly DataUltimaGraduacao,
    string   Email,
    string?  Telefone = null
) : IRequest;
