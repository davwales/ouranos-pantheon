"use client";

import { SymbolTable } from "@/app/(plutus)/plutus/[marketId]/recipes/_components/symbol-table";
import { type SelectedSymbol } from "@/app/(plutus)/plutus/components/symbol-search";
import { type PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { useShallow } from "zustand/react/shallow";
import { RecipeForm } from "../_components/recipe-form";
import { type RecipeSymbol } from "../types";
import { NotFoundCard } from "@/components/shared/not-found-card";
import { useUnsavedChangesGuard } from "@/hooks/use-unsaved-changes-guard";
import { hasChanges } from "@/lib/has-changes";
import { RecipeDetailHeader } from "./_components/recipe-detail-header";
import { RecipeDetailSkeleton } from "./_components/recipe-detail-skeleton";

export default function RecipeDetailPage() {
  const { marketId, recipeId } = useParams<{
    marketId: string;
    recipeId: string;
  }>();
  const [name, setName] = useState("");
  const [cost, setCost] = useState(0);
  const [inputs, setInputs] = useState<RecipeSymbol[]>([]);
  const [outputs, setOutputs] = useState<RecipeSymbol[]>([]);
  const [selectedSymbols, setSelectedSymbols] = useState<SelectedSymbol[]>([]);
  const [isInputDialogOpen, setIsInputDialogOpen] = useState(false);
  const [isOutputDialogOpen, setIsOutputDialogOpen] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);

  const router = useRouter();

  const [timeFrameKey] = usePlutusStore(
    useShallow((state: PlutusState) => [state.timeFrameKey]),
  );

  const [state, reexecute] = useApi(
    () => plutusApi.getRecipe(recipeId, timeFrameKey),
    [recipeId, timeFrameKey],
  );

  const recipe = state.data;
  useBreadcrumbLabel(recipeId, recipe?.name, state.status === "error");

  const editable = { name, cost, inputs, outputs };
  const [savedRecipe, setSavedRecipe] = useState<typeof editable>();
  useUnsavedChangesGuard(
    savedRecipe !== undefined && hasChanges(editable, savedRecipe),
  );

  const [syncedRecipe, setSyncedRecipe] = useState(recipe);
  if (recipe !== syncedRecipe) {
    setSyncedRecipe(recipe);
    if (recipe) {
      setName(recipe.name);
      setCost(recipe.cost);
      setInputs(recipe.inputs);
      setOutputs(recipe.outputs);
      setSavedRecipe({
        name: recipe.name,
        cost: recipe.cost,
        inputs: recipe.inputs,
        outputs: recipe.outputs,
      });
    }
  }

  const handleDelete = async () => {
    setIsProcessing(true);
    try {
      await plutusApi.deleteRecipe(recipeId);
      router.push(`/plutus/${marketId}/recipes`);
    } catch (error) {
      console.error("Failed to delete recipe:", error);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleSave = async () => {
    setIsProcessing(true);
    try {
      await plutusApi.updateRecipe({
        recipeId,
        marketId,
        name,
        cost,
        inputs: inputs.map((input) => ({
          symbolId: input.symbolId,
          name: input.name,
          quantity: input.quantity,
        })),
        outputs: outputs.map((output) => ({
          symbolId: output.symbolId,
          name: output.name,
          quantity: output.quantity,
        })),
      });
      setSavedRecipe(editable);
    } catch (error) {
      console.error("Failed to save recipe:", error);
    } finally {
      setIsProcessing(false);
    }
  };

  if (state.status === "error" && !recipe) {
    return <NotFoundCard title="Recipe not found" backHref={`/plutus/${marketId}/recipes`} backLabel="Back to Recipes" error={state.error} onRetry={reexecute} />;
  }

  if (state.status === "loading" && !recipe) {
    return <RecipeDetailSkeleton />;
  }

  return (
    <div>
      <RecipeDetailHeader
        isProcessing={isProcessing}
        onSave={handleSave}
        onDelete={handleDelete}
      />

      <RecipeForm
        name={name}
        cost={cost}
        onNameChange={setName}
        onCostChange={setCost}
      />

      <div className="space-y-6 mt-4">
        <SymbolTable
          marketId={marketId}
          title="Inputs"
          items={inputs}
          onItemsChange={setInputs}
          isDialogOpen={isInputDialogOpen}
          onDialogOpenChange={setIsInputDialogOpen}
          selectedSymbols={selectedSymbols}
          onSymbolsChanged={setSelectedSymbols}
        />

        <SymbolTable
          marketId={marketId}
          title="Outputs"
          items={outputs}
          onItemsChange={setOutputs}
          isDialogOpen={isOutputDialogOpen}
          onDialogOpenChange={setIsOutputDialogOpen}
          selectedSymbols={selectedSymbols}
          onSymbolsChanged={setSelectedSymbols}
        />
      </div>
    </div>
  );
}
