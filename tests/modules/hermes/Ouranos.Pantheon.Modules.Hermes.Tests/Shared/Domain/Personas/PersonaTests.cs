using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Personas;

public sealed class PersonaTests
{
    private readonly IFixture _fixture = new Fixture();

    public PersonaTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Persona>(Guid.NewGuid().ToString());
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var personality = _fixture.Create<string>();
        var scenario = _fixture.Create<string>();
        var isDefault = _fixture.Create<bool>();
        var isPublic = _fixture.Create<bool>();

        // Act
        var persona = Persona.Create(
            id,
            name,
            description,
            personality,
            scenario,
            isDefault,
            isPublic
        );

        // Assert
        persona.Id.ShouldBe(id);
        persona.Name.ShouldBe(name);
        persona.Description.ShouldBe(description);
        persona.Personality.ShouldBe(personality);
        persona.Scenario.ShouldBe(scenario);
        persona.IsDefault.ShouldBe(isDefault);
        persona.IsPublic.ShouldBe(isPublic);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = new Id<Persona>(Guid.NewGuid().ToString());

        // Act
        var create = () => Persona.Create(id, name!, _fixture.Create<string>());

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidDescription_ShouldThrowArgumentException(string? description)
    {
        // Arrange
        var id = new Id<Persona>(Guid.NewGuid().ToString());

        // Act
        var create = () => Persona.Create(id, _fixture.Create<string>(), description!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var id = new Id<Persona>(Guid.NewGuid().ToString());
        var persona = Persona.Create(id, _fixture.Create<string>(), _fixture.Create<string>());
        var newName = _fixture.Create<string>();
        var newDescription = _fixture.Create<string>();
        var newPersonality = _fixture.Create<string>();
        var newScenario = _fixture.Create<string>();

        // Act
        persona.Update(newName, newDescription, newPersonality, newScenario, true, false);

        // Assert
        persona.Name.ShouldBe(newName);
        persona.Description.ShouldBe(newDescription);
        persona.Personality.ShouldBe(newPersonality);
        persona.Scenario.ShouldBe(newScenario);
        persona.IsDefault.ShouldBeTrue();
        persona.IsPublic.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? newName)
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () => persona.Update(newName!, _fixture.Create<string>());

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidDescription_ShouldThrowArgumentException(string? newDescription)
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () => persona.Update(_fixture.Create<string>(), newDescription!);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }
}
