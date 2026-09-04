"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Typography } from "@/components/shared/typography";
import { hestiaApi } from "@/lib/api/hestia";
import type { ShoppingListResponse } from "@/lib/api/hestia-types";
import { useEffect, useRef, useState } from "react";
import { AddManualItemForm } from "./add-manual-item-form";
import { IngredientChecklist } from "./ingredient-checklist";
import { ManualItemsList } from "./manual-items-list";
import { ShoppingListRecipes } from "./shopping-list-recipes";
import Link from "next/link";

export type ShoppingListViewProps = {
  data: ShoppingListResponse;
  onReload: () => void;
};

const DEBOUNCE_MS = 400;

export function ShoppingListView({ data, onReload }: ShoppingListViewProps) {
  const [checked, setChecked] = useState<Set<string>>(
    () => new Set(data.checkedItemIds),
  );
  const [updateError, setUpdateError] = useState<string | null>(null);
  const [isAdding, setIsAdding] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    // why: a manual add/delete reloads the list and resets local state to the
    // server truth; any pending debounced PUT is dropped intentionally.
    setChecked(new Set(data.checkedItemIds));
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
      debounceRef.current = null;
    }
  }, [data.checkedItemIds]);

  const flushUpdate = async (nextChecked: Set<string>) => {
    try {
      await hestiaApi.updateCheckedItems({
        checkedItemIds: Array.from(nextChecked),
      });
      setUpdateError(null);
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to update checked items",
      );
      onReload();
    }
  };

  const handleToggle = (lineId: string) => {
    setUpdateError(null);
    setChecked((prev) => {
      const next = new Set(prev);
      if (next.has(lineId)) {
        next.delete(lineId);
      } else {
        next.add(lineId);
      }

      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
      debounceRef.current = setTimeout(() => {
        void flushUpdate(next);
      }, DEBOUNCE_MS);

      return next;
    });
  };

  const handleAddManualItem = async (text: string) => {
    setUpdateError(null);
    setIsAdding(true);
    try {
      await hestiaApi.addManualItem({ text });
      onReload();
      return true;
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to add item",
      );
      return false;
    } finally {
      setIsAdding(false);
    }
  };

  const handleDeleteManualItem = async (itemId: string) => {
    setUpdateError(null);
    try {
      await hestiaApi.deleteManualItem(itemId);
      onReload();
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to delete item",
      );
    }
  };

  const handleRemoveRecipe = async (recipeId: string) => {
    setUpdateError(null);
    try {
      await hestiaApi.toggleRecipeInShoppingList(recipeId);
      onReload();
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to remove recipe",
      );
    }
  };

  const handleClearRecipes = async () => {
    setUpdateError(null);
    try {
      // why: sequential to avoid concurrent writes to the shared list document.
      for (const recipe of data.recipes) {
        await hestiaApi.toggleRecipeInShoppingList(recipe.id);
      }
      onReload();
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to clear recipes",
      );
      onReload();
    }
  };

  useEffect(() => {
    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, []);

  const hasIngredients = data.consolidatedIngredients.length > 0;
  const hasManualItems = data.manualItems.length > 0;
  const isEmpty = !hasIngredients && !hasManualItems;

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-2">
        <Typography variant="h2" className="border-b-0">
          Shopping List
        </Typography>
        {data.recipeIds.length > 0 && (
          <p className="text-sm text-muted-foreground">
            Ingredients from {data.recipeIds.length}{" "}
            {data.recipeIds.length === 1 ? "recipe" : "recipes"}
          </p>
        )}
      </div>

      <AddManualItemForm onAdd={handleAddManualItem} adding={isAdding} />

      {updateError && (
        <div
          role="alert"
          className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
        >
          {updateError}
        </div>
      )}

      {isEmpty ? (
        <div className="rounded-lg border bg-card p-6 text-center text-muted-foreground">
          Your shopping list is empty. Save recipes from the{" "}
          <Button variant="link" asChild className="h-auto p-0">
            <Link href="/hestia/recipes">Recipes</Link>
          </Button>{" "}
          page to build your grocery list.
        </div>
      ) : (
        <div className="space-y-4">
          {data.recipes.length > 0 && (
            <ShoppingListRecipes
              recipes={data.recipes}
              onRemove={handleRemoveRecipe}
              onClearAll={handleClearRecipes}
            />
          )}
          {hasIngredients && (
            <Card>
              <CardHeader>
                <CardTitle>Ingredients</CardTitle>
              </CardHeader>
              <CardContent>
                <IngredientChecklist
                  ingredients={data.consolidatedIngredients}
                  checked={checked}
                  onToggle={handleToggle}
                />
              </CardContent>
            </Card>
          )}
          {hasManualItems && (
            <Card>
              <CardHeader>
                <CardTitle>Manual Items</CardTitle>
              </CardHeader>
              <CardContent>
                <ManualItemsList
                  items={data.manualItems}
                  checked={checked}
                  onToggle={handleToggle}
                  onDelete={handleDeleteManualItem}
                />
              </CardContent>
            </Card>
          )}
        </div>
      )}
    </div>
  );
}
