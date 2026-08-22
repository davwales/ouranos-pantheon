using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.GetAvailableModels;

public sealed class GetAvailableModelsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAvailableModelsHandler _handler;
    private readonly ILogger<GetAvailableModelsHandler> _logger = Substitute.For<
        ILogger<GetAvailableModelsHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public GetAvailableModelsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAvailableModelsHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenAvailableModelsExist_ShouldReturnAllAvailableModels()
    {
        // Arrange
        var model1 = AvailableModel.Create(
            DatabaseExtensions.CreateId<AvailableModel>(),
            "llama3.2",
            "meta"
        );
        var model2 = AvailableModel.Create(
            DatabaseExtensions.CreateId<AvailableModel>(),
            "gpt-4o",
            "openai"
        );

        await _dbContext.SeedData(model1, model2);

        var query = new GetAvailableModelsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<List<GetAvailableModelsResponse>>();
        result.Count.ShouldBe(2);

        var firstItem = result[0];
        firstItem.Id.ShouldNotBe(default);
        firstItem.ModelIdentifier.ShouldBe("gpt-4o");
        firstItem.OwnedBy.ShouldBe("openai");

        var secondItem = result[1];
        secondItem.Id.ShouldNotBe(default);
        secondItem.ModelIdentifier.ShouldBe("llama3.2");
        secondItem.OwnedBy.ShouldBe("meta");
    }

    [Fact]
    public async Task Handle_WhenNoAvailableModels_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAvailableModelsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAvailableModelsInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
