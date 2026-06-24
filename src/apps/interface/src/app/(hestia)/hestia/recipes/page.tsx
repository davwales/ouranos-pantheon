"use client";

import { InfoCardGridSkeleton } from "@/components/shared/skeletons/info-card-skeleton";
import { useApi } from "@/hooks/use-api";
import { hestiaApi } from "@/lib/api/hestia";
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

  return <RecipesView recipes={state.data?.items ?? []} />;
}
