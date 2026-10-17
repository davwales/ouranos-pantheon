using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Loras;

public sealed class LoraTests
{
    private readonly Fixture _fixture = new();

    public LoraTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Lora>(Guid.NewGuid().ToString());
        var name = _fixture.Create<string>();
        var identifier = _fixture.Create<string>();
        var isPublic = _fixture.Create<bool>();

        // Act
        var lora = Lora.Create(id, name, identifier, 0.75f, isPublic);

        // Assert
        lora.Id.ShouldBe(id);
        lora.Name.ShouldBe(name);
        lora.Identifier.ShouldBe(identifier);
        lora.Weight.ShouldBe(0.75f);
        lora.IsPublic.ShouldBe(isPublic);
    }

    [Fact]
    public void Create_WhenWeightOmitted_ShouldUseDefaultWeight()
    {
        // Arrange
        var id = new Id<Lora>(Guid.NewGuid().ToString());

        // Act
        var lora = Lora.Create(id, _fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        lora.Weight.ShouldBe(Lora.DefaultWeight);
    }

    [Fact]
    public void Create_WhenNameAndIdentifierPadded_ShouldTrimThem()
    {
        // Arrange
        var id = new Id<Lora>(Guid.NewGuid().ToString());

        // Act
        var lora = Lora.Create(id, "  Detail  ", "  add-detail-xl  ");

        // Assert
        lora.Name.ShouldBe("Detail");
        lora.Identifier.ShouldBe("add-detail-xl");
    }

    [Theory]
    [InlineData(-2.01f)]
    [InlineData(2.01f)]
    public void Create_WhenWeightOutOfRange_ShouldThrowArgumentOutOfRangeException(float weight)
    {
        // Arrange
        var id = new Id<Lora>(Guid.NewGuid().ToString());

        // Act
        var create = () =>
            Lora.Create(id, _fixture.Create<string>(), _fixture.Create<string>(), weight);

        // Assert
        create.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = new Id<Lora>(Guid.NewGuid().ToString());

        // Act
        var create = () => Lora.Create(id, name!, _fixture.Create<string>());

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidIdentifier_ShouldThrowArgumentException(string? identifier)
    {
        // Arrange
        var id = new Id<Lora>(Guid.NewGuid().ToString());

        // Act
        var create = () => Lora.Create(id, _fixture.Create<string>(), identifier!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var id = new Id<Lora>(Guid.NewGuid().ToString());
        var lora = Lora.Create(id, _fixture.Create<string>(), _fixture.Create<string>());
        var newName = _fixture.Create<string>();
        var newIdentifier = _fixture.Create<string>();
        var newIsPublic = !lora.IsPublic;

        // Act
        lora.Update(newName, newIdentifier, -0.5f, newIsPublic);

        // Assert
        lora.Name.ShouldBe(newName);
        lora.Identifier.ShouldBe(newIdentifier);
        lora.Weight.ShouldBe(-0.5f);
        lora.IsPublic.ShouldBe(newIsPublic);
    }

    [Theory]
    [InlineData(-3f)]
    [InlineData(3f)]
    public void Update_WhenWeightOutOfRange_ShouldThrowArgumentOutOfRangeException(float weight)
    {
        // Arrange
        var lora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () =>
            lora.Update(_fixture.Create<string>(), _fixture.Create<string>(), weight);

        // Assert
        update.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? newName)
    {
        // Arrange
        var lora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () => lora.Update(newName!, _fixture.Create<string>(), Lora.DefaultWeight);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidIdentifier_ShouldThrowArgumentException(string? newIdentifier)
    {
        // Arrange
        var lora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () =>
            lora.Update(_fixture.Create<string>(), newIdentifier!, Lora.DefaultWeight);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }
}
