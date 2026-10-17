using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
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
}
