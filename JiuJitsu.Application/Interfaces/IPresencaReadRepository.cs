using JiuJitsu.Application.DTOs;

namespace JiuJitsu.Application.Interfaces;

public interface IPresencaReadRepository
{
    Task<ListaPresencasDto> ListarPorTurmaAsync(
        Guid turmaId, DateOnly dataInicio, DateOnly dataFim,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<FrequenciaAtletaDto>> FrequenciaPorTurmaAsync(
        Guid turmaId, DateOnly dataInicio, DateOnly dataFim,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<FrequenciaAtletaDto>> FrequenciaPorAtletaAsync(
        Guid atletaId, DateOnly dataInicio, DateOnly dataFim,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AtletaInativoDto>> ListarInativosAsync(
        Guid? filialId, int diasSemPresenca,
        CancellationToken cancellationToken = default);
}
