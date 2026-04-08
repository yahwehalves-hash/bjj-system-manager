using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Configuracoes.Queries.ObterConfiguracaoEfetiva;

public record ObterConfiguracaoEfetivaQuery(Guid FilialId) : IRequest<ConfiguracaoEfetivaDto>;
