using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.ToolSets;

public sealed class ToolSetValidationTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();

    [Fact]
    public async Task ValidateToolSetExistsAsync_WhenToolSetIdNull_ShouldNotThrow()
    {
        // Act
        var validate = async () =>
            await ToolSetValidation.ValidateToolSetExistsAsync(_dbContext, null);

        // Assert
        await validate.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ValidateToolSetExistsAsync_WhenToolSetExists_ShouldNotThrow()
    {
        // Arrange
        var toolSet = ToolSet.Create(DatabaseExtensions.CreateId<ToolSet>(), "Set", null, null, []);
        await _dbContext.SeedData(toolSet);

        // Act
        var validate = async () =>
            await ToolSetValidation.ValidateToolSetExistsAsync(_dbContext, toolSet.Id);

        // Assert
        await validate.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ValidateToolSetExistsAsync_WhenToolSetDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var toolSetId = DatabaseExtensions.CreateId<ToolSet>();

        // Act
        var validate = async () =>
            await ToolSetValidation.ValidateToolSetExistsAsync(_dbContext, toolSetId);

        // Assert
        await validate.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateToolSetExistsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var toolSetId = DatabaseExtensions.CreateId<ToolSet>();

        // Act
        var validate = async () =>
            await ToolSetValidation.ValidateToolSetExistsAsync(
                _dbContext,
                toolSetId,
                new CancellationToken(true)
            );

        // Assert
        await validate.ShouldThrowAsync<OperationCanceledException>();
    }
}
