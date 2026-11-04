using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.DeleteModel;

public sealed class DeleteModelHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteModelHandler _handler;
    private readonly ILogger<DeleteModelHandler> _logger = Substitute.For<
        ILogger<DeleteModelHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public DeleteModelHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new DeleteModelHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteModelAndReturnId()
    {
        // Arrange
        var existingModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(existingModel);

        var command = new DeleteModelInput(existingModel.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<DeleteModelResponse>();
        result.ModelId.ShouldBe(existingModel.Id);

        var deletedModel = await _dbContext.ModelConfigs.FindAsync(existingModel.Id);
        deletedModel.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenModelNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteModelInput(new Id<ModelConfig>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteModelInput(new Id<ModelConfig>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenModelIsUsedByConversations_ShouldThrowConflictException()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            "Llama",
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(model);
        foreach (var _ in Enumerable.Range(0, 2))
        {
            await _dbContext.SeedData(
                Conversation.Create(
                    new Id<Conversation>(Guid.NewGuid().ToString()),
                    new Id<Persona>(Guid.NewGuid().ToString()),
                    model.Id,
                    [],
                    [],
                    _fixture.Create<string>()
                )
            );
        }

        // Act
        var handle = async () =>
            await _handler.Handle(new DeleteModelInput(model.Id), CancellationToken.None);

        // Assert
        var exception = await handle.ShouldThrowAsync<ConflictException>();
        exception.Message.ShouldContain("'Llama' is used by 2 conversations.");
        (await _dbContext.ModelConfigs.FindAsync(model.Id)).ShouldNotBeNull();
    }
}
