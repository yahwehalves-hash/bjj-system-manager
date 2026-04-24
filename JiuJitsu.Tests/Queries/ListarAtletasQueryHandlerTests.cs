using Bogus;
using FluentAssertions;
using JiuJitsu.Application.DTOs;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Application.Queries.ListarAtletas;
using NSubstitute;

namespace JiuJitsu.Tests.Queries;

public class ListarAtletasQueryHandlerTests
{
    private readonly IAtletaReadRepository _readRepository = Substitute.For<IAtletaReadRepository>();
    private readonly ListarAtletasQueryHandler _handler;
    private readonly Faker _faker = new("pt_BR");

    public ListarAtletasQueryHandlerTests()
    {
        _handler = new ListarAtletasQueryHandler(_readRepository);
    }

    [Fact]
    public async Task Handle_DeveRetornarListaPaginada()
    {
        // Arrange
        var query = new ListarAtletasQuery(Nome: null, Faixa: null, Pagina: 1, TamanhoPagina: 10);

        var itens = Enumerable.Range(1, 1).Select(_ => new AtletaResumoDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _faker.Person.FullName,
            "12345678901",
            "Azul",
            2,
            "Aluno",
            true)).ToList();

        var resultadoEsperado = new ListaAtletasDto(
            Itens: itens,
            TotalItens: 1,
            Pagina: 1,
            TamanhoPagina: 10);

        _readRepository
            .ListarAsync(null, null, 1, 10, Arg.Any<CancellationToken>())
            .Returns(resultadoEsperado);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.TotalItens.Should().Be(1);
        resultado.Itens.Should().HaveCount(1);
        resultado.Pagina.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DevePassarFiltrosCorretamenteAoRepositorio()
    {
        // Arrange
        var query = new ListarAtletasQuery(Nome: "carlos", Faixa: "Azul", Pagina: 2, TamanhoPagina: 5);

        _readRepository
            .ListarAsync("carlos", "Azul", 2, 5, Arg.Any<CancellationToken>())
            .Returns(new ListaAtletasDto([], 0, 2, 5));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _readRepository.Received(1).ListarAsync("carlos", "Azul", 2, 5, Arg.Any<CancellationToken>());
    }
}
