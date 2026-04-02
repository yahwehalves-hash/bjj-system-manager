using FluentAssertions;
using JiuJitsu.Application.Commands.ExcluirAtleta;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using NSubstitute;

namespace JiuJitsu.Tests.Commands;

public class ExcluirAtletaCommandHandlerTests
{
    private readonly IMessagePublisher     _publisher      = Substitute.For<IMessagePublisher>();
    private readonly IAtletaReadRepository _readRepository = Substitute.For<IAtletaReadRepository>();
    private readonly ExcluirAtletaCommandHandler _handler;

    public ExcluirAtletaCommandHandlerTests()
    {
        _handler = new ExcluirAtletaCommandHandler(_publisher, _readRepository);
    }

    [Fact]
    public async Task Handle_DevePublicarMensagemDeExclusaoQuandoAtletaExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        _readRepository.ExistePorIdAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        await _handler.Handle(new ExcluirAtletaCommand(id), CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublicarAsync(
            Arg.Is<AtletaMensagem>(m => m.Operacao == "Exclusao" && m.AtletaId == id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeveLancarExcecaoQuandoAtletaNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();
        _readRepository.ExistePorIdAsync(id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var acao = () => _handler.Handle(new ExcluirAtletaCommand(id), CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<KeyNotFoundException>();
        await _publisher.DidNotReceive().PublicarAsync(Arg.Any<AtletaMensagem>(), Arg.Any<CancellationToken>());
    }
}
