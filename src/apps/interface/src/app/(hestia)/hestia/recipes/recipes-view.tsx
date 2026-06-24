import { InfoCard } from "@/components/shared/info-card";
import type { RecipeSummary } from "@/lib/api/hestia-types";

export type RecipesViewProps = {
  recipes: RecipeSummary[];
};

export function RecipesView({ recipes }: RecipesViewProps) {
  if (recipes.length === 0) {
    return (
      <div className="m-4 text-center text-muted-foreground">
        No recipes found. Add your first recipe!
      </div>
    );
  }

  return (
    <div className="m-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {recipes.map((recipe) => {
        const card = (
          <InfoCard
            label={recipe.title}
            description={recipe.sourceUrl ?? "No source"}
            className="hover:bg-accent h-full w-full"
          />
        );

        return recipe.sourceUrl ? (
          <a
            key={recipe.id}
            href={recipe.sourceUrl}
            target="_blank"
            rel="noopener noreferrer"
          >
            {card}
          </a>
        ) : (
          <div key={recipe.id}>{card}</div>
        );
      })}
    </div>
  );
}
