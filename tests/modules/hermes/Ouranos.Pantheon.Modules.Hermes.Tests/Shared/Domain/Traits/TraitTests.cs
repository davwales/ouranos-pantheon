using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Traits;

public sealed class TraitTests
{
    private readonly IFixture _fixture = new Fixture();

    public TraitTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Trait>(Guid.NewGuid().ToString());
        var name = _fixture.Create<string>();
        var content = _fixture.Create<string>();
        var isPublic = _fixture.Create<bool>();

        // Act
        var trait = Trait.Create(id, name, content, isPublic);

        // Assert
        trait.Id.ShouldBe(id);
        trait.Name.ShouldBe(name);
        trait.Content.ShouldBe(content);
        trait.IsPublic.ShouldBe(isPublic);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = new Id<Trait>(Guid.NewGuid().ToString());

        // Act
        var create = () => Trait.Create(id, name!, _fixture.Create<string>());

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidContent_ShouldThrowArgumentException(string? content)
    {
        // Arrange
        var id = new Id<Trait>(Guid.NewGuid().ToString());

        // Act
        var create = () => Trait.Create(id, _fixture.Create<string>(), content!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var id = new Id<Trait>(Guid.NewGuid().ToString());
        var trait = Trait.Create(id, _fixture.Create<string>(), _fixture.Create<string>());
        var newName = _fixture.Create<string>();
        var newContent = _fixture.Create<string>();
        var newIsPublic = !trait.IsPublic;

        // Act
        trait.Update(newName, newContent, newIsPublic);

        // Assert
        trait.Name.ShouldBe(newName);
        trait.Content.ShouldBe(newContent);
        trait.IsPublic.ShouldBe(newIsPublic);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? newName)
    {
        // Arrange
        var trait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () => trait.Update(newName!, _fixture.Create<string>());

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidContent_ShouldThrowArgumentException(string? newContent)
    {
        // Arrange
        var trait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () => trait.Update(_fixture.Create<string>(), newContent!);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }
}
