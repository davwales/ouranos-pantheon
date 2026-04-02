using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.CreateTrait;

public sealed class CreateTraitHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateTraitHandler _handler;
    private readonly ILogger<CreateTraitHandler> _logger = Substitute.For<ILogger<CreateTraitHandler>>();
    private readonly HermesDbContext _dbContext;

    public CreateTraitHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new CreateTraitHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateTraitAndReturnId()
    {
        // Arrange
        var command = new CreateTraitInput(_fixture.Create<string>(), _fixture.Create<string>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreateTraitResponse>();
        result.TraitId.ShouldNotBe(default);

        var savedTrait = await _dbContext.Traits.FindAsync(result.TraitId);
        savedTrait.ShouldNotBeNull();
        savedTrait.Name.ShouldBe(command.Name);
        savedTrait.Content.ShouldBe(command.Content);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateTraitInput(_fixture.Create<string>(), _fixture.Create<string>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
