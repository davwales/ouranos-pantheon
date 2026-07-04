"use client";

import { Button } from "@/components/ui/button";
import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";
import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import { hestiaApi } from "@/lib/api/hestia";
import Link from "next/link";
import { RecipesView } from "./recipes-view";

export default function RecipesPage() {
  const [state] = useApi(() => hestiaApi.getAllRecipes({ take: 50 }));

  if (state.status === "loading" && !state.data) {
    return <InfoCardGridSkeleton />;
  }

  if (state.status === "error") {
    return (
      <div className="m-4 text-center text-destructive">
        Failed to load recipes: {state.error?.message}
      </div>
    );
  }

  return (
    <div className="m-4 space-y-4">
      <div className="flex items-center justify-between">
        <Typography variant="h2" className="border-b-0">
          Recipes
        </Typography>
        <Button asChild>
          <Link href="/hestia/recipes/create">New Recipe</Link>
        </Button>
      </div>
      <RecipesView recipes={state.data?.items ?? []} />
    </div>
  );
}
