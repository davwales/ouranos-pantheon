"use client";

import { Typography } from "@/app/components/typography";
import { SelectedSymbol } from "@/app/plutus/components/symbol-search";
import { DELETE_RECIPE, UPDATE_RECIPE } from "@/app/plutus/mutations";
import { GET_RECIPE_DETAILS } from "@/app/plutus/queries";
import { RecipeFormFields } from "@/app/plutus/recipes/[marketId]/[recipeId]/components/recipe-form";
import { SymbolTable } from "@/app/plutus/recipes/[marketId]/[recipeId]/components/symbol-table";
import { RecipeSymbol } from "@/app/plutus/recipes/[marketId]/[recipeId]/types";
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

  const handleAddInput = () => {
    if (selectedSymbol) {
      setInputs([
        ...inputs,
        { name: selectedSymbol.name, quantity: 1, symbolId: selectedSymbol.id },
      ]);
      setIsInputDialogOpen(false);
      setSelectedSymbol(undefined);
    }
  };

  const handleRemoveInput = (index: number) => {
    setInputs(inputs.filter((_, i) => i !== index));
  };

  const handleInputQuantityChange = (index: number, quantity: number) => {
    const newInputs = [...inputs];
    newInputs[index].quantity = quantity;
    setInputs(newInputs);
  };

  const handleAddOutput = () => {
    if (selectedSymbol) {
      setOutputs([
        ...outputs,
        { name: selectedSymbol.name, quantity: 1, symbolId: selectedSymbol.id },
      ]);
      setIsOutputDialogOpen(false);
      setSelectedSymbol(undefined);
    }
  };

  const handleRemoveOutput = (index: number) => {
    setOutputs(outputs.filter((_, i) => i !== index));
  };

  const handleOutputQuantityChange = (index: number, quantity: number) => {
    const newOutputs = [...outputs];
    newOutputs[index].quantity = quantity;
    setOutputs(newOutputs);
  };

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
          <Button
            onClick={handleDelete}
            disabled={isProcessing}
            variant="destructive"
          >
            Delete
          </Button>
        </div>
      </div>

      <RecipeFormFields
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
          onAdd={handleAddInput}
          onRemove={handleRemoveInput}
          onQuantityChange={handleInputQuantityChange}
          isDialogOpen={isInputDialogOpen}
          onDialogOpenChange={setIsInputDialogOpen}
          selectedSymbol={selectedSymbol}
          onSymbolSelected={setSelectedSymbol}
        />

        <SymbolTable
          marketId={marketId}
          title="Outputs"
          items={outputs}
          onAdd={handleAddOutput}
          onRemove={handleRemoveOutput}
          onQuantityChange={handleOutputQuantityChange}
          isDialogOpen={isOutputDialogOpen}
          onDialogOpenChange={setIsOutputDialogOpen}
          selectedSymbol={selectedSymbol}
          onSymbolSelected={setSelectedSymbol}
        />
      </div>
    </div>
  );
}
