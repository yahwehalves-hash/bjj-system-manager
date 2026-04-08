using JiuJitsu.Application.DTOs;
using MediatR;

namespace JiuJitsu.Application.Configuracoes.Queries.ObterConfiguracaoGlobal;

public record ObterConfiguracaoGlobalQuery : IRequest<ConfiguracaoGlobalDto?>;
