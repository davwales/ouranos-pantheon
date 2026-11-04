using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Personas;

public sealed class PersonaValidationTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();

    private readonly Persona _persona = Persona.Create(
        DatabaseExtensions.CreateId<Persona>(),
        "Sage",
        "A wise persona."
    );

    [Fact]
    public async Task ValidatePersonaExistsAsync_WhenPersonaExists_ShouldNotThrow()
    {
        // Arrange
        await _dbContext.SeedData(_persona);

        // Act
        var validate = async () =>
            await PersonaValidation.ValidatePersonaExistsAsync(_dbContext, _persona.Id);

        // Assert
        await validate.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ValidatePersonaExistsAsync_WhenPersonaDoesNotExist_ShouldThrowNotFoundException()
    {
        // Act
        var validate = async () =>
            await PersonaValidation.ValidatePersonaExistsAsync(_dbContext, _persona.Id);

        // Assert
        var exception = await validate.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldContain("persona");
    }

    [Fact]
    public async Task ValidatePersonaIsUnusedAsync_WhenNoConversationUsesIt_ShouldNotThrow()
    {
        // Arrange
        await _dbContext.SeedData(_persona);

        // Act
        var validate = async () =>
            await PersonaValidation.ValidatePersonaIsUnusedAsync(_dbContext, _persona);

        // Assert
        await validate.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task ValidatePersonaIsUnusedAsync_WhenConversationsUseIt_ShouldThrowConflictException()
    {
        // Arrange
        await _dbContext.SeedData(_persona);
        foreach (var _ in Enumerable.Range(0, 2))
        {
            await _dbContext.SeedData(
                Conversation.Create(
                    DatabaseExtensions.CreateId<Conversation>(),
                    _persona.Id,
                    DatabaseExtensions.CreateId<ModelConfig>(),
                    [],
                    [],
                    "Chat"
                )
            );
        }

        // Act
        var validate = async () =>
            await PersonaValidation.ValidatePersonaIsUnusedAsync(_dbContext, _persona);

        // Assert
        var exception = await validate.ShouldThrowAsync<ConflictException>();
        exception.Message.ShouldBe(
            "The persona 'Sage' is used by 2 conversations. "
                + "Switch them to another persona or delete them first."
        );
    }

    [Fact]
    public async Task ValidatePersonaExistsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Act
        var validate = async () =>
            await PersonaValidation.ValidatePersonaExistsAsync(
                _dbContext,
                _persona.Id,
                new CancellationToken(true)
            );

        // Assert
        await validate.ShouldThrowAsync<OperationCanceledException>();
    }
}
