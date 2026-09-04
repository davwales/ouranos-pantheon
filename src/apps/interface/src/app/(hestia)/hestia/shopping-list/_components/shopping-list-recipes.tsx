"use client";

import { ConfirmationButton } from "@/components/shared/confirmation-button";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { ShoppingListRecipe } from "@/lib/api/hestia-types";
import { X } from "lucide-react";
import Link from "next/link";

export type ShoppingListRecipesProps = {
  recipes: ShoppingListRecipe[];
  onRemove: (recipeId: string) => void;
  onClearAll: () => void;
};

export function ShoppingListRecipes({
  recipes,
  onRemove,
  onClearAll,
}: ShoppingListRecipesProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Recipes</CardTitle>
      </CardHeader>
      <CardContent>
        <ul role="list" className="divide-y divide-border">
          {recipes.map((recipe) => (
            <li key={recipe.id}>
              <div className="flex items-center justify-between gap-3 py-3 px-1">
                <Link
                  href={`/hestia/recipes/${recipe.id}`}
                  className="text-base hover:underline"
                >
                  {recipe.title}
                </Link>
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={`Remove ${recipe.title}`}
                  onClick={() => onRemove(recipe.id)}
                >
                  <X className="size-4" />
                </Button>
              </div>
            </li>
          ))}
        </ul>
        <div className="pt-4">
          <ConfirmationButton
            variant="outline"
            size="sm"
            title="Clear all recipes"
            description="Removes all recipes and their ingredients from your shopping list."
            onConfirm={onClearAll}
          >
            Clear all recipes
          </ConfirmationButton>
        </div>
      </CardContent>
    </Card>
  );
}