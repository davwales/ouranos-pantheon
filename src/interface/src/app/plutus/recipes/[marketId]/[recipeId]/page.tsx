"use client";

import { ConfirmationButton } from "@/app/components/confirmation-button";
import { Typography } from "@/app/components/typography";
import { SelectedSymbol } from "@/app/plutus/components/symbol-search";
import { DELETE_RECIPE, UPDATE_RECIPE } from "@/app/plutus/mutations";
import { GET_RECIPE_DETAILS } from "@/app/plutus/queries";
import { RecipeForm } from "@/app/plutus/recipes/components/recipe-form";
import { SymbolTable } from "@/app/plutus/recipes/components/symbol-table";
import { RecipeSymbol } from "@/app/plutus/recipes/types";
import { Button } from "@/components/ui/button";
import { useMutation, useQuery } from "@urql/next";
import { RefreshCw } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";

export default function RecipeDetailPage() {
  const { marketId, recipeId } = useParams<{
    marketId: string;
    recipeId: string;
  }>();
  const [name, setName] = useState("");
  const [cost, setCost] = useState(0);
  const [inputs, setInputs] = useState<RecipeSymbol[]>([]);
  const [outputs, setOutputs] = useState<RecipeSymbol[]>([]);
  const [selectedSymbol, setSelectedSymbol] = useState<SelectedSymbol>();
  const [isInputDialogOpen, setIsInputDialogOpen] = useState(false);
  const [isOutputDialogOpen, setIsOutputDialogOpen] = useState(false);

  const router = useRouter();

  const [{ data, fetching }] = useQuery({
    query: GET_RECIPE_DETAILS,
    variables: {
      recipeId: recipeId,
    },
  });

  const [{ fetching: isSaving }, updateRecipe] = useMutation<
    { idResponseOfRecipe: { id: string } },
    {
      input: {
        recipeId: string;
        marketId: string;
        name: string;
        cost: number;
        inputs: Array<{ symbolId: string; name: string; quantity: number }>;
        outputs: Array<{ symbolId: string; name: string; quantity: number }>;
      };
    }
  >(UPDATE_RECIPE);

  const [{ fetching: isDeleting }, deleteRecipe] = useMutation<
    { idResponseOfRecipe: { id: string } },
    { input: { recipeId: string } }
  >(DELETE_RECIPE);

  const isProcessing = isSaving || isDeleting;

  useEffect(() => {
    if (data?.recipe) {
      setName(data.recipe.name);
      setCost(data.recipe.cost);
      setInputs(data.recipe.inputs);
      setOutputs(data.recipe.outputs);
    }
  }, [data?.recipe]);

  const handleDelete = async () => {
    try {
      await deleteRecipe({
        input: {
          recipeId,
        },
      });

      router.push(`/plutus/recipes/${marketId}`);
    } catch (error) {
      console.error("Failed to delete recipe:", error);
    }
  };

  const handleSave = async () => {
    try {
      await updateRecipe({
        input: {
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
        },
      });
    } catch (error) {
      console.error("Failed to save recipe:", error);
    }
  };

  if (fetching) {
    return <div>Loading...</div>;
  }

  if (!data?.recipe) {
    return <div>Recipe not found</div>;
  }

  return (
    <div>
      <div className="flex items-center justify-between">
        <Typography variant="h2" className="border-b-0">
          Recipe Details
        </Typography>

        <div className="flex items-center gap-2">
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
          selectedSymbol={selectedSymbol}
          onSymbolSelected={setSelectedSymbol}
        />

        <SymbolTable
          marketId={marketId}
          title="Outputs"
          items={outputs}
          onItemsChange={setOutputs}
          isDialogOpen={isOutputDialogOpen}
          onDialogOpenChange={setIsOutputDialogOpen}
          selectedSymbol={selectedSymbol}
          onSymbolSelected={setSelectedSymbol}
        />
      </div>
    </div>
  );
}
