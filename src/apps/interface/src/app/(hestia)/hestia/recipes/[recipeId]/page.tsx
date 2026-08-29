"use client";

import { NotFoundCard } from "@/components/shared/not-found-card";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use-interval";
import { hestiaApi } from "@/lib/api/hestia";
import type { Recipe } from "@/lib/api/hestia-types";
import { useParams } from "next/navigation";
import { useCallback, useState } from "react";
import { RecipeDetailSkeleton } from "./_components/recipe-detail-skeleton";
import { RecipeEditView } from "./_components/recipe-edit-view";
import { RecipeHeader } from "./_components/recipe-header";
import { RecipeImportFailedView } from "./_components/recipe-import-failed-view";
import { RecipeImportingView } from "./_components/recipe-importing-view";
import { RecipeReadOnlyView } from "./_components/recipe-read-only-view";

export default function RecipeDetailPage() {
  const { recipeId } = useParams<{ recipeId: string }>();
  const [recipe, reexecute] = useApi<Recipe>(
    () => hestiaApi.getRecipe(recipeId),
    [recipeId],
  );
  const [shoppingList, reexecuteShoppingList] = useApi(
    () => hestiaApi.getShoppingList(),
    [],
  );
  const [isEditing, setIsEditing] = useState(false);
  const [isReimporting, setIsReimporting] = useState(false);
  const [reimportError, setReimportError] = useState<string | null>(null);
  const [toggleError, setToggleError] = useState<string | null>(null);

  const isImporting = recipe.data?.importStatus === "Importing";
  useInterval(
    useCallback(() => reexecute(), [reexecute]),
    isImporting ? 3000 : null,
  );

  const handleReimport = async () => {
    setIsReimporting(true);
    setReimportError(null);
    try {
      await hestiaApi.reimportRecipe(recipeId);
      setIsEditing(false);
      reexecute();
    } catch (err) {
      setReimportError(
        err instanceof Error ? err.message : "Failed to reimport recipe",
      );
    } finally {
      setIsReimporting(false);
    }
  };

  const handleToggleInList = async () => {
    setToggleError(null);
    try {
      await hestiaApi.toggleRecipeInShoppingList(recipeId);
      reexecuteShoppingList();
    } catch (err) {
      setToggleError(
        err instanceof Error ? err.message : "Failed to update shopping list",
      );
    }
  };

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
  const isInList = shoppingList.data?.recipeIds.includes(recipeId) ?? false;

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
        onReimport={handleReimport}
        isReimporting={isReimporting}
        isInList={isInList}
        onToggleInList={handleToggleInList}
      />
      {reimportError && (
        <div
          role="alert"
          className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
        >
          {reimportError}
        </div>
      )}
      {toggleError && (
        <div
          role="alert"
          className="rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive"
        >
          {toggleError}
        </div>
      )}
      {data.importStatus === "Importing" ? (
        <RecipeImportingView />
      ) : data.importStatus === "Failed" ? (
        <RecipeImportFailedView
          errorMessage={data.importFailureReason}
          onReimport={handleReimport}
          isReimporting={isReimporting}
        />
      ) : isEditing ? (
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