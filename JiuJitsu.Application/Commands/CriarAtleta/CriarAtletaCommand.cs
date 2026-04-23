using JiuJitsu.Domain.Enums;
using MediatR;

namespace JiuJitsu.Application.Commands.CriarAtleta;

// Command de criação — retorna o ID gerado para que a API possa informar ao cliente
public record CriarAtletaCommand(
    Guid              FilialId,
    string            NomeCompleto,
    string            Cpf,
    DateOnly          DataNascimento,
    Faixa             Faixa,
    Grau              Grau,
    DateOnly          DataUltimaGraduacao,
    string            Email,
    string?           Telefone         = null,
    TipoAtleta        TipoAtleta       = TipoAtleta.Aluno,
    CanalNotificacao  CanalNotificacao = CanalNotificacao.Email
) : IRequest<Guid>;
