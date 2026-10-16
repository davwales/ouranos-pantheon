using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.AskKitchenAssistant.Schemas;

public sealed record KitchenAssistantContext(Id<Recipe> RecipeId) : IRecipeScope;
