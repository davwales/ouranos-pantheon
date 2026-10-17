using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.DeleteLora;

public sealed class DeleteLoraHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteLoraHandler _handler;
    private readonly ILogger<DeleteLoraHandler> _logger = Substitute.For<
        ILogger<DeleteLoraHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public DeleteLoraHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new DeleteLoraHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteLoraAndReturnId()
    {
        // Arrange
        var existingLora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(existingLora);

        var command = new DeleteLoraInput(existingLora.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<DeleteLoraResponse>();
        result.LoraId.ShouldBe(existingLora.Id);

        var deletedLora = await _dbContext.Loras.FindAsync(existingLora.Id);
        deletedLora.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenLoraNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteLoraInput(new Id<Lora>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteLoraInput(new Id<Lora>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
