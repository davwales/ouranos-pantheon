using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;
using Lora = Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras.Lora;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.GetAllLoras;

public sealed class GetAllLorasHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllLorasHandler _handler;
    private readonly ILogger<GetAllLorasHandler> _logger = Substitute.For<
        ILogger<GetAllLorasHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetAllLorasHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAllLorasHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenLorasExist_ShouldReturnAllLoras()
    {
        // Arrange
        var lora1 = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        var lora2 = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: false
        );

        await _dbContext.SeedData(lora1, lora2);

        var query = new GetAllLorasInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<List<GetAllLorasResponse>>();
        result.Count.ShouldBe(2);
        var item = result[0];
        item.Id.ShouldNotBe(default);
        item.Name.ShouldNotBeNull();
        item.Identifier.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoLoras_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllLorasInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPublicModeEnabled_ShouldOnlyReturnPublicLoras()
    {
        // Arrange
        var publicLora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        var privateLora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: false
        );

        await _dbContext.SeedData(publicLora, privateLora);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetAllLorasInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(publicLora.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllLorasInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenLorasExist_ShouldReturnResponseWithAllProperties()
    {
        // Arrange
        await _dbContext.SeedData(
            Lora.Create(
                new Id<Lora>(Guid.NewGuid().ToString()),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                isPublic: false
            )
        );

        var query = new GetAllLorasInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsPublic.ShouldBeFalse();
    }

    private static AvailableLora Available(string identifier, bool isCompatible)
    {
        return AvailableLora.Create(
            new Id<AvailableLora>(Guid.NewGuid().ToString()),
            identifier,
            null,
            null,
            null,
            null,
            null,
            isCompatible
        );
    }

    [Fact]
    public async Task Handle_ShouldReportWhichLorasAreUnavailableOrIncompatible()
    {
        // Arrange
        await _dbContext.SeedData(
            Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "A", "ok", triggerPhrase: "ok"),
            Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "B", "gone"),
            Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "C", "mismatch")
        );
        await _dbContext.SeedData(Available("ok", true), Available("mismatch", false));

        // Act
        var result = await _handler.Handle(new GetAllLorasInput(), CancellationToken.None);

        // Assert
        result
            .Select(l => (l.Identifier, l.TriggerPhrase, l.IsUnavailable, l.IsIncompatible))
            .ShouldBe([
                ("ok", "ok", false, false),
                ("gone", null, true, false),
                ("mismatch", null, false, true),
            ]);
    }
}
