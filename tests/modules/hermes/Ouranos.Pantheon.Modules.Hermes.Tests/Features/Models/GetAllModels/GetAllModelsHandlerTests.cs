using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.GetAllModels;

public sealed class GetAllModelsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllModelsHandler _handler;
    private readonly ILogger<GetAllModelsHandler> _logger = Substitute.For<ILogger<GetAllModelsHandler>>();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetAllModelsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAllModelsHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenModelsExist_ShouldReturnAllModels()
    {
        // Arrange
        var model1 = ModelConfig.Create(new Id<ModelConfig>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: true);
        var model2 = ModelConfig.Create(new Id<ModelConfig>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: false);
        await _dbContext.SeedData(model1, model2);

        var query = new GetAllModelsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<List<GetAllModelsResponse>>();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenNoModels_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllModelsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPublicModeEnabled_ShouldOnlyReturnPublicModels()
    {
        // Arrange
        var publicModel = ModelConfig.Create(new Id<ModelConfig>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: true);
        var privateModel = ModelConfig.Create(new Id<ModelConfig>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: false);
        await _dbContext.SeedData(publicModel, privateModel);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetAllModelsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(publicModel.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllModelsInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
