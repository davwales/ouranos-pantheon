"use client";

import { Typography } from "@/app/components/typography";
import { SelectedSymbol } from "@/app/plutus/components/symbol-search";
import { CREATE_RECIPE } from "@/app/plutus/mutations";
import { RecipeForm } from "@/app/plutus/recipes/components/recipe-form";
import { SymbolTable } from "@/app/plutus/recipes/components/symbol-table";
import { RecipeSymbol } from "@/app/plutus/recipes/types";
import { Button } from "@/components/ui/button";
import { useMutation } from "@urql/next";
import { RefreshCw } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

export default function CreateRecipePage() {
  const { marketId } = useParams<{ marketId: string }>();
  const router = useRouter();
  const [name, setName] = useState("");
  const [cost, setCost] = useState(0);
  const [inputs, setInputs] = useState<RecipeSymbol[]>([]);
  const [outputs, setOutputs] = useState<RecipeSymbol[]>([]);
  const [selectedSymbol, setSelectedSymbol] = useState<SelectedSymbol>();
  const [isInputDialogOpen, setIsInputDialogOpen] = useState(false);
  const [isOutputDialogOpen, setIsOutputDialogOpen] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);

  const [, createRecipe] = useMutation(CREATE_RECIPE);

  const handleSubmit = async () => {
    setIsProcessing(true);
    try {
      const response = await createRecipe({
        input: {
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

      if (response.data?.createRecipe.idResponseOfRecipe?.id) {
        const recipeId = response.data.createRecipe.idResponseOfRecipe.id;
        router.replace(`/plutus/recipes/${marketId}/${recipeId}`);
      }
    } catch (error) {
      console.error("Failed to create recipe:", error);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between">
        <Typography variant="h2" className="border-b-0">
          Create New Recipe
        </Typography>

        <div className="flex items-center gap-2">
          {isProcessing && <RefreshCw className="animate-spin" />}
          <Button onClick={handleSubmit} disabled={isProcessing}>
            Save
          </Button>
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
