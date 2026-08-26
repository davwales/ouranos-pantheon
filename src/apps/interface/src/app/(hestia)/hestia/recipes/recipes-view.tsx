import { InfoCard } from "@/components/shared/info-card";
import Link from "next/link";
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
      {recipes.map((recipe) => (
        <Link key={recipe.id} href={`/hestia/recipes/${recipe.id}`}>
          <InfoCard
            label={recipe.title}
            description={recipe.sourceUrl ?? "No source"}
            className="hover:bg-accent h-full w-full"
          />
        </Link>
      ))}
    </div>
  );
}
