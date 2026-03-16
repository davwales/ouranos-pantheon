using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;

public sealed record GetAllRecipesInput : IQuery<WrapperResponse<IQueryable<Recipe>>>;
