using FluentAssertions;
using JiuJitsu.Application.Commands.CriarAtleta;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Enums;
using NSubstitute;

namespace JiuJitsu.Tests.Commands;

public class CriarAtletaCommandHandlerTests
{
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
    private readonly CriarAtletaCommandHandler _handler;

    public CriarAtletaCommandHandlerTests()
    {
        _handler = new CriarAtletaCommandHandler(_publisher);
    }

    [Fact]
    public async Task Handle_DevePublicarMensagemComOperacaoCriacao()
    {
        // Arrange
        var command = new CriarAtletaCommand(
            NomeCompleto:        "Carlos Silva",
            Cpf:                 "12345678901",
            DataNascimento:      new DateOnly(1990, 5, 15),
            Faixa:               Faixa.Azul,
            Grau:                Grau.Segundo,
            DataUltimaGraduacao: new DateOnly(2023, 1, 10),
            Email:               "carlos@email.com");

        // Act
        var id = await _handler.Handle(command, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty("deve retornar um GUID válido");

        await _publisher.Received(1).PublicarAsync(
            Arg.Is<AtletaMensagem>(m =>
                m.Operacao == "Criacao" &&
                m.AtletaId == id &&
                m.Payload!.NomeCompleto == "Carlos Silva" &&
                m.Payload.Email == "carlos@email.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeveChamarPublisherExatamenteUmaVez()
    {
        // Arrange
        var command = new CriarAtletaCommand(
            "Ana Lima", "98765432100",
            new DateOnly(1995, 3, 20),
            Faixa.Roxa, Grau.Primeiro,
            new DateOnly(2024, 6, 1),
            "ana@email.com");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublicarAsync(Arg.Any<AtletaMensagem>(), Arg.Any<CancellationToken>());
    }
}
