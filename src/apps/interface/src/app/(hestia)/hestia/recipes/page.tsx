"use client";

import { Button } from "@/components/ui/button";
import { RecipeCardGridSkeleton } from "./_components/recipe-card-skeleton";
import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import { hestiaApi } from "@/lib/api/hestia";
import Link from "next/link";
import { useState } from "react";
import { ImportRecipeDialog } from "./_components/import-recipe-dialog";
import { RecipesView } from "./recipes-view";

export default function RecipesPage() {
  const [state] = useApi(() => hestiaApi.getAllRecipes({ take: 50 }));
  const [list, reexecuteList] = useApi(() => hestiaApi.getShoppingList(), []);
  const [toggleError, setToggleError] = useState<string | null>(null);
  const [importOpen, setImportOpen] = useState(false);

  const selectedRecipeIds = new Set(list.data?.recipeIds ?? []);
  const items = state.data?.items ?? [];
  const totalCount = state.data?.totalCount ?? 0;
  const isLoading = state.status === "loading" && !state.data;

  const handleToggleRecipe = async (recipeId: string) => {
    setToggleError(null);
    try {
      await hestiaApi.toggleRecipeInShoppingList(recipeId);
      reexecuteList();
    } catch (err) {
      setToggleError(
        err instanceof Error ? err.message : "Failed to update shopping list",
      );
    }
  };

  return (
    <div className="m-4 space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <Typography variant="h2" className="border-b-0">
          Recipes
        </Typography>
        <div className="flex flex-wrap gap-3">
          <Button variant="outline" onClick={() => setImportOpen(true)}>
            Import from Link
          </Button>
          <Button asChild>
            <Link href="/hestia/recipes/create">New Recipe</Link>
          </Button>
        </div>
      </div>
      {toggleError && (
        <div
          role="alert"
          className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
        >
          {toggleError}
        </div>
      )}
      {isLoading ? (
        <RecipeCardGridSkeleton count={9} />
      ) : state.status === "error" && !state.data ? (
        <div role="alert" className="text-center text-destructive">
          Failed to load recipes: {state.error?.message}
        </div>
      ) : (
        <>
          <RecipesView
            recipes={items}
            inListRecipeIds={selectedRecipeIds}
            onToggleRecipe={handleToggleRecipe}
          />
          {totalCount > items.length && (
            <p className="text-sm text-muted-foreground">
              Showing {items.length} of {totalCount} recipes
            </p>
          )}
        </>
      )}
      <ImportRecipeDialog open={importOpen} onOpenChange={setImportOpen} />
    </div>
  );
}