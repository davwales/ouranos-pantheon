using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;

public sealed record RecipeTitleChanged(string Title) : IDomainEvent;
