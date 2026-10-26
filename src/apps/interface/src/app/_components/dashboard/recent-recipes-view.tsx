"use client";

import { useApi } from "@/hooks/use-api";
import { hestiaApi } from "@/lib/api/hestia";
import { formatDistanceToNow } from "date-fns";
import { ChefHat } from "lucide-react";
import { DashboardCard } from "./dashboard-card";
import { DashboardRow } from "./dashboard-row";

const RECIPES_SHOWN = 5;

export function RecentRecipesView() {
  const [state] = useApi(
    () => hestiaApi.getAllRecipes({ take: RECIPES_SHOWN }),
    [],
  );

  return (
    <DashboardCard
      title="Recent Recipes"
      icon={ChefHat}
      href="/hestia/recipes"
      state={state}
      isEmpty={(data) => data.items.length === 0}
      emptyText="No recipes saved yet."
    >
      {(data) => (
        <ul>
          {data.items.map((recipe) => (
            <DashboardRow
              key={recipe.id}
              href={`/hestia/recipes/${recipe.id}`}
              primary={recipe.title}
              secondary={`Added ${formatDistanceToNow(new Date(recipe.createdAt), { addSuffix: true })}`}
              trailing={
                <span className="text-xs text-muted-foreground">
                  {recipe.ingredientCount} ingredients
                </span>
              }
            />
          ))}
        </ul>
      )}
    </DashboardCard>
  );
}
