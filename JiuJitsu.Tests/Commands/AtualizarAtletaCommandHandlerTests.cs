using Bogus;
using FluentAssertions;
using JiuJitsu.Application.Commands.AtualizarAtleta;
using JiuJitsu.Application.Interfaces;
using JiuJitsu.Contracts.Mensagens;
using JiuJitsu.Domain.Enums;
using NSubstitute;

namespace JiuJitsu.Tests.Commands;

public class AtualizarAtletaCommandHandlerTests
{
    private readonly IMessagePublisher    _publisher      = Substitute.For<IMessagePublisher>();
    private readonly IAtletaReadRepository _readRepository = Substitute.For<IAtletaReadRepository>();
    private readonly AtualizarAtletaCommandHandler _handler;
    private readonly Faker _faker = new("pt_BR");

    public AtualizarAtletaCommandHandlerTests()
    {
        _handler = new AtualizarAtletaCommandHandler(_publisher, _readRepository);
    }

    private AtualizarAtletaCommand GerarCommand(Guid? id = null)
    {
        return new AtualizarAtletaCommand(
            Id:                  id ?? Guid.NewGuid(),
            NomeCompleto:        _faker.Person.FullName,
            DataNascimento:      DateOnly.FromDateTime(_faker.Date.Past(20, DateTime.Now.AddYears(-5))),
            Faixa:               _faker.PickRandom<Faixa>(),
            Grau:                _faker.PickRandom<Grau>(),
            DataUltimaGraduacao: DateOnly.FromDateTime(_faker.Date.Past(2)),
            Email:               _faker.Internet.Email(),
            Telefone:            _faker.Phone.PhoneNumber("###########")
        );
    }

    [Fact]
    public async Task Handle_DevePublicarMensagemDeAtualizacao_QuandoAtletaExiste()
    {
        // Arrange
        var command = GerarCommand();
        _readRepository.ExistePorIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _publisher.Received(1).PublicarAsync(
            Arg.Is<AtletaMensagem>(m =>
                m.Operacao == "Atualizacao" &&
                m.AtletaId == command.Id &&
                m.Payload!.NomeCompleto == command.NomeCompleto &&
                m.Payload.Email == command.Email),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoAtletaNaoExiste()
    {
        // Arrange
        var command = GerarCommand();
        _readRepository.ExistePorIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var acao = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Atleta com Id '{command.Id}' não encontrado.");
        
        await _publisher.DidNotReceive().PublicarAsync(Arg.Any<AtletaMensagem>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_DeveLancarExcecao_QuandoNomeForInvalido(string? nomeInvalido)
    {
        // Arrange
        var command = GerarCommand() with { NomeCompleto = nomeInvalido! };

        // Act
        var acao = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Nome completo é obrigatório.");
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoDataNascimentoForFutura()
    {
        // Arrange
        var command = GerarCommand() with { DataNascimento = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) };

        // Act
        var acao = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Data de nascimento deve ser no passado.");
    }

    [Fact]
    public async Task Handle_DeveLancarExcecao_QuandoEmailForInvalido()
    {
        // Arrange
        var command = GerarCommand() with { Email = "email-invalido" };

        // Act
        var acao = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Formato de email inválido. (Parameter 'valor')");
    }
}
