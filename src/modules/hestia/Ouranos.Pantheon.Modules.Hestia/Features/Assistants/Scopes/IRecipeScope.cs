using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Scopes;

/// <summary>
/// What a tool needs from an assistant's context to work on one recipe. Tools are typed to the
/// narrowest scope they need, so any assistant whose context implements it can register them.
/// </summary>
public interface IRecipeScope
{
    Id<Recipe> RecipeId { get; }
}
