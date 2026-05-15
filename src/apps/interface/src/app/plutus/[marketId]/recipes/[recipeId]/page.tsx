"use client";

import { ConfirmationButton } from "@/app/components/confirmation-button";
import { Typography } from "@/app/components/typography";
import { SymbolTable } from "@/app/plutus/[marketId]/recipes/components/symbol-table";
import { SelectedSymbol } from "@/app/plutus/components/symbol-search";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useShallow } from "zustand/react/shallow";
import { RecipeForm } from "../components/recipe-form";
import { RecipeSymbol } from "../types";
import { DetailSkeleton } from "@/app/components/skeletons/detail-skeleton";
import { FormSkeleton } from "@/app/components/skeletons/form-skeleton";
import { Skeleton } from "@/components/ui/skeleton";

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

  const [state] = useApi(
    () => plutusApi.getRecipe(recipeId, timeFrameKey),
    [recipeId, timeFrameKey],
  );

  const recipe = state.data;
  const fetching = state.status === "loading";

  useEffect(() => {
    if (recipe) {
      setName(recipe.name);
      setCost(recipe.cost);
      setInputs(recipe.inputs);
      setOutputs(recipe.outputs);
    }
  }, [recipe]);

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
    } catch (error) {
      console.error("Failed to save recipe:", error);
    } finally {
      setIsProcessing(false);
    }
  };

  if (fetching) {
    return (
      <div aria-hidden="true">
        <DetailSkeleton sections={["header"]} />
        <FormSkeleton fields={2} hasTitle={false} className="mt-4 max-w-lg" />
        <div className="space-y-6 mt-4">
          <div>
            <Skeleton className="h-5 w-24 mb-2" />
            <div className="border rounded-lg">
              <div className="border-b p-3">
                <Skeleton className="h-4 w-full" />
              </div>
              <div className="p-3">
                <div className="grid grid-cols-3 gap-4">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-16" />
                  <Skeleton className="h-4 w-16" />
                </div>
              </div>
            </div>
          </div>
          <div>
            <Skeleton className="h-5 w-24 mb-2" />
            <div className="border rounded-lg">
              <div className="border-b p-3">
                <Skeleton className="h-4 w-full" />
              </div>
              <div className="p-3">
                <div className="grid grid-cols-3 gap-4">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-16" />
                  <Skeleton className="h-4 w-16" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!recipe) {
    return <div>Recipe not found</div>;
  }

  return (
    <div>
      <div className="flex items-center justify-between">
        <Typography variant="h2" className="border-b-0">
          Recipe Details
        </Typography>

        <div className="flex items-center gap-2">
          <TimeFrameSelection />
          {isProcessing && <RefreshCw className="animate-spin" />}
          <Button onClick={handleSave} disabled={isProcessing}>
            Save
          </Button>
          <ConfirmationButton
            title="Delete Recipe"
            description="Are you sure you want to delete this recipe? This action cannot be undone."
            onConfirm={handleDelete}
            disabled={isProcessing}
            variant="destructive"
          >
            Delete
          </ConfirmationButton>
        </div>
      </div>

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
