import { RecipeCard } from "./_components/recipe-card";
import type { RecipeSummary } from "@/lib/api/hestia-types";

export type RecipesViewProps = {
  recipes: RecipeSummary[];
  inListRecipeIds: Set<string>;
  onToggleRecipe: (recipeId: string) => void;
};

export function RecipesView({
  recipes,
  inListRecipeIds,
  onToggleRecipe,
}: RecipesViewProps) {
  if (recipes.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No recipes found. Add your first recipe!
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {recipes.map((recipe) => (
        <RecipeCard
          key={recipe.id}
          recipe={recipe}
          isInList={inListRecipeIds.has(recipe.id)}
          onToggle={onToggleRecipe}
        />
      ))}
    </div>
  );
}