using Bogus;
using FluentAssertions;
using JiuJitsu.Application.Commands.CriarAtleta;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using NSubstitute;

namespace JiuJitsu.Tests.Commands;

public class CriarAtletaCommandHandlerTests
{
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
    private readonly IAtletaRepository _atletaRepository = Substitute.For<IAtletaRepository>();
    private readonly CriarAtletaCommandHandler _handler;
    private readonly Faker _faker = new("pt_BR");

    public CriarAtletaCommandHandlerTests()
    {
        _atletaRepository.ExisteCpfAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _atletaRepository.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        _handler = new CriarAtletaCommandHandler(_publisher, _atletaRepository);
    }

    private CriarAtletaCommand GerarCommand()
    {
        return new CriarAtletaCommand(
            FilialId:            Guid.NewGuid(),
            NomeCompleto:        _faker.Person.FullName,
            Cpf:                 "12345678901",
            DataNascimento:      DateOnly.FromDateTime(_faker.Date.Past(20, DateTime.Now.AddYears(-5))),
            Faixa:               _faker.PickRandom<Faixa>(),
            Grau:                _faker.PickRandom<Grau>(),
            DataUltimaGraduacao: DateOnly.FromDateTime(_faker.Date.Past(2)),
            Email:               _faker.Internet.Email());
    }

    [Fact]
    public async Task Handle_DevePublicarMensagemComOperacaoCriacao()
    {
        // Arrange
        var command = GerarCommand();

        // Act
        var id = await _handler.Handle(command, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty("deve retornar um GUID válido");

        await _publisher.Received(1).PublicarAsync(
            Arg.Is<AtletaMensagem>(m =>
                m.Operacao == "Criacao" &&
                m.AtletaId == id &&
                m.Payload!.NomeCompleto == command.NomeCompleto &&
                m.Payload.Email == command.Email),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeveChamarPublisherExatamenteUmaVez()
    {
        // Arrange
        var command = GerarCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublicarAsync(Arg.Any<AtletaMensagem>(), Arg.Any<CancellationToken>());
    }
}
