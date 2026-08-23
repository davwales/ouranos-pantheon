"use client";

import { NotFoundCard } from "@/components/shared/not-found-card";
import { useApi } from "@/hooks/use-api";
import { hestiaApi } from "@/lib/api/hestia";
import type { Recipe } from "@/lib/api/hestia-types";
import { useParams } from "next/navigation";
import { useState } from "react";
import { RecipeDetailSkeleton } from "./_components/recipe-detail-skeleton";
import { RecipeEditView } from "./_components/recipe-edit-view";
import { RecipeHeader } from "./_components/recipe-header";
import { RecipeReadOnlyView } from "./_components/recipe-read-only-view";

export default function RecipeDetailPage() {
  const { recipeId } = useParams<{ recipeId: string }>();
  const [recipe, reexecute] = useApi<Recipe>(
    () => hestiaApi.getRecipe(recipeId),
    [recipeId],
  );
  const [isEditing, setIsEditing] = useState(false);

  if (recipe.status === "error" && !recipe.data) {
    return (
      <NotFoundCard
        title="Recipe not found"
        message="This recipe doesn't exist or has been removed."
        backHref="/hestia/recipes"
        backLabel="Back to Recipes"
      />
    );
  }

  if (!recipe.data) {
    return <RecipeDetailSkeleton />;
  }

  const data = recipe.data;

  return (
    <div className="m-4 space-y-6">
      <RecipeHeader
        data={data}
        isEditing={isEditing}
        onEdit={() => setIsEditing(true)}
        onCancel={() => setIsEditing(false)}
        onReverted={() => {
          reexecute();
          setIsEditing(false);
        }}
      />
      {isEditing ? (
        <RecipeEditView
          data={data}
          onCancel={() => setIsEditing(false)}
          onSaved={() => {
            reexecute();
            setIsEditing(false);
          }}
        />
      ) : (
        <RecipeReadOnlyView data={data} />
      )}
    </div>
  );
}