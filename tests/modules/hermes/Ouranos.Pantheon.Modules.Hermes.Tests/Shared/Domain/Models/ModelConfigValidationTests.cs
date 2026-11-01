using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Models;

public sealed class ModelConfigValidationTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();

    private readonly ModelConfig _modelConfig = ModelConfig.Create(
        DatabaseExtensions.CreateId<ModelConfig>(),
        "Llama",
        "llama3.2",
        "System prompt."
    );

    [Fact]
    public async Task ValidateModelConfigExistsAsync_WhenModelExists_ShouldNotThrow()
    {
        // Arrange
        await _dbContext.SeedData(_modelConfig);

        // Act
        var validate = async () =>
            await ModelConfigValidation.ValidateModelConfigExistsAsync(_dbContext, _modelConfig.Id);

        // Assert
        await validate.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ValidateModelConfigExistsAsync_WhenModelDoesNotExist_ShouldThrowNotFoundException()
    {
        // Act
        var validate = async () =>
            await ModelConfigValidation.ValidateModelConfigExistsAsync(_dbContext, _modelConfig.Id);

        // Assert
        var exception = await validate.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldContain("model");
    }

    [Fact]
    public async Task ValidateModelConfigIsUnusedAsync_WhenNoConversationUsesIt_ShouldNotThrow()
    {
        // Arrange
        await _dbContext.SeedData(_modelConfig);

        // Act
        var validate = async () =>
            await ModelConfigValidation.ValidateModelConfigIsUnusedAsync(_dbContext, _modelConfig);

        // Assert
        await validate.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ValidateModelConfigIsUnusedAsync_WhenAConversationUsesIt_ShouldThrowConflictException()
    {
        // Arrange
        await _dbContext.SeedData(_modelConfig);
        await _dbContext.SeedData(
            Conversation.Create(
                DatabaseExtensions.CreateId<Conversation>(),
                DatabaseExtensions.CreateId<Persona>(),
                _modelConfig.Id,
                [],
                [],
                "Chat"
            )
        );

        // Act
        var validate = async () =>
            await ModelConfigValidation.ValidateModelConfigIsUnusedAsync(_dbContext, _modelConfig);

        // Assert
        var exception = await validate.ShouldThrowAsync<ConflictException>();
        exception.Message.ShouldBe(
            "The model 'Llama' is used by 1 conversation. "
                + "Switch them to another model or delete them first."
        );
    }

    [Fact]
    public async Task ValidateModelConfigExistsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Act
        var validate = async () =>
            await ModelConfigValidation.ValidateModelConfigExistsAsync(
                _dbContext,
                _modelConfig.Id,
                new CancellationToken(true)
            );

        // Assert
        await validate.ShouldThrowAsync<OperationCanceledException>();
    }
}
