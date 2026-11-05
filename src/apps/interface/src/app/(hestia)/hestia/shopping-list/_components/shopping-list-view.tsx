"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Typography } from "@/components/shared/typography";
import { hestiaApi } from "@/lib/api/hestia";
import type { ShoppingCategory, ShoppingListResponse } from "@/lib/api/hestia-types";
import { useEffect, useRef, useState } from "react";
import { AddManualItemForm } from "./add-manual-item-form";
import { toChecklistItems } from "./checklist-items";
import { ShoppingChecklist } from "./shopping-checklist";
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

  const [syncedCheckedIds, setSyncedCheckedIds] = useState(data.checkedItemIds);
  if (data.checkedItemIds !== syncedCheckedIds) {
    setSyncedCheckedIds(data.checkedItemIds);
    setChecked(new Set(data.checkedItemIds));
  }

  const reload = () => {
    // why: a reload resets local state to the server truth; any pending
    // debounced PUT is dropped intentionally.
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
      debounceRef.current = null;
    }
    onReload();
  };

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
      reload();
    }
  };

  // Sends a debounced checked-items update now, so a request that depends on it sees the latest state.
  const flushPendingUpdate = async () => {
    if (!debounceRef.current) {
      return;
    }
    clearTimeout(debounceRef.current);
    debounceRef.current = null;
    await hestiaApi.updateCheckedItems({ checkedItemIds: Array.from(checked) });
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
      reload();
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
      reload();
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
      reload();
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
      reload();
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to clear recipes",
      );
      reload();
    }
  };

  const handleClearChecked = async () => {
    setUpdateError(null);
    try {
      await flushPendingUpdate();
      await hestiaApi.clearCheckedItems();
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to clear checked items",
      );
    }
    reload();
  };

  const handleCategoryChange = async (
    name: string,
    category: ShoppingCategory | null,
  ) => {
    setUpdateError(null);
    try {
      await flushPendingUpdate();
      await hestiaApi.setItemCategory({ name, category });
      reload();
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to set category",
      );
    }
  };

  useEffect(() => {
    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, []);

  const items = toChecklistItems(data);
  const isEmpty = items.length === 0;

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
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle>Items</CardTitle>
              <Button
                variant="outline"
                size="sm"
                disabled={checked.size === 0}
                onClick={handleClearChecked}
              >
                Clear checked
              </Button>
            </CardHeader>
            <CardContent>
              <ShoppingChecklist
                items={items}
                checked={checked}
                onToggle={handleToggle}
                onDelete={handleDeleteManualItem}
                onCategoryChange={handleCategoryChange}
              />
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
