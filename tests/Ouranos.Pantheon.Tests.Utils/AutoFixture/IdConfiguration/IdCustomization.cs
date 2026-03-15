using AutoFixture;

namespace Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

public sealed class IdCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(new IdSpecimenBuilder());
    }
}