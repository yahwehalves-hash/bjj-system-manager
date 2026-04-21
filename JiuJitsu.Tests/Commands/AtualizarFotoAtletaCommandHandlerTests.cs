using Bogus;
using FluentAssertions;
using JiuJitsu.Application.Commands.AtualizarFotoAtleta;
using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Domain.ValueObjects;
using NSubstitute;

namespace JiuJitsu.Tests.Commands;

public class AtualizarFotoAtletaCommandHandlerTests
{
    private readonly IAtletaRepository _atletaRepository = Substitute.For<IAtletaRepository>();
    private readonly AtualizarFotoAtletaCommandHandler _handler;
    private readonly Faker _faker = new("pt_BR");

    public AtualizarFotoAtletaCommandHandlerTests()
    {
        _handler = new AtualizarFotoAtletaCommandHandler(_atletaRepository);
    }

    private Atleta GerarAtleta()
    {
        return new Atleta(
            filialId:            Guid.NewGuid(),
            nomeCompleto:        _faker.Person.FullName,
            cpf:                 new Cpf("12345678901"),
            dataNascimento:      DateOnly.FromDateTime(_faker.Date.Past(20, DateTime.Now.AddYears(-5))),
            faixa:               _faker.PickRandom<Faixa>(),
            grau:                _faker.PickRandom<Grau>(),
            dataUltimaGraduacao: DateOnly.FromDateTime(_faker.Date.Past(2)),
            email:               new Email(_faker.Internet.Email())
        );
    }

    [Fact]
    public async Task Handle_DeveAtualizarFoto_QuandoAtletaExiste()
    {
        // Arrange
        var atleta = GerarAtleta();
        var fotoBase64 = Convert.ToBase64String(_faker.Random.Bytes(100));
        var command = new AtualizarFotoAtletaCommand(atleta.Id, fotoBase64);

        _atletaRepository.ObterPorIdAsync(atleta.Id, Arg.Any<CancellationToken>()).Returns(atleta);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        atleta.FotoBase64.Should().Be(fotoBase64);
        await _atletaRepository.Received(1).AtualizarAsync(atleta, Arg.Any<CancellationToken>());
        await _atletaRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoAtletaNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new AtualizarFotoAtletaCommand(id, "foto");
        _atletaRepository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns((Atleta?)null);

        // Act
        var acao = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Atleta com Id '{id}' não encontrado.");
        
        await _atletaRepository.DidNotReceive().AtualizarAsync(Arg.Any<Atleta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeveRemoverFoto_QuandoFotoForNull()
    {
        // Arrange
        var atleta = GerarAtleta();
        atleta.AtualizarFoto("foto-antiga");
        var command = new AtualizarFotoAtletaCommand(atleta.Id, null);

        _atletaRepository.ObterPorIdAsync(atleta.Id, Arg.Any<CancellationToken>()).Returns(atleta);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        atleta.FotoBase64.Should().BeNull();
        await _atletaRepository.Received(1).AtualizarAsync(atleta, Arg.Any<CancellationToken>());
        await _atletaRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
}
