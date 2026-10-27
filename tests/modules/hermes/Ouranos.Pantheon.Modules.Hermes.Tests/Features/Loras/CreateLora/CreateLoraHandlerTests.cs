using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.CreateLora;

public sealed class CreateLoraHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateLoraHandler _handler;
    private readonly ILogger<CreateLoraHandler> _logger = Substitute.For<
        ILogger<CreateLoraHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public CreateLoraHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new CreateLoraHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateLoraAndReturnId()
    {
        // Arrange
        var command = new CreateLoraInput(_fixture.Create<string>(), _fixture.Create<string>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreateLoraResponse>();
        result.LoraId.ShouldNotBe(default);

        var savedLora = await _dbContext.Loras.FindAsync(result.LoraId);
        savedLora.ShouldNotBeNull();
        savedLora.Name.ShouldBe(command.Name);
        savedLora.Identifier.ShouldBe(command.Identifier);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateLoraInput(_fixture.Create<string>(), _fixture.Create<string>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenTriggerPhraseIsGiven_ShouldSaveIt()
    {
        // Arrange
        var command = new CreateLoraInput("Detail", "add-detail", TriggerPhrase: "detailed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedLora = await _dbContext.Loras.FindAsync(result.LoraId);
        savedLora.ShouldNotBeNull().TriggerPhrase.ShouldBe("detailed");
    }

    [Fact]
    public async Task Handle_WhenTheLoraAlreadyHasAConfig_ShouldThrowConflictException()
    {
        // Arrange
        await _dbContext.SeedData(
            Lora.Create(DatabaseExtensions.CreateId<Lora>(), "Detail", "add-detail")
        );

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() =>
            _handler.Handle(new CreateLoraInput("Again", "add-detail"), CancellationToken.None)
        );
    }
}
