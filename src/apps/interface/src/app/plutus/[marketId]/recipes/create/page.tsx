"use client";

import { Typography } from "@/app/components/typography";
import { SymbolTable } from "@/app/plutus/[marketId]/recipes/components/symbol-table";
import { SelectedSymbol } from "@/app/plutus/components/symbol-search";
import { Button } from "@/components/ui/button";
import { plutusApi } from "@/lib/api/plutus";
import { RefreshCw } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { RecipeForm } from "../components/recipe-form";
import { RecipeSymbol } from "../types";

export default function CreateRecipePage() {
  const { marketId } = useParams<{ marketId: string }>();
  const router = useRouter();
  const [name, setName] = useState("");
  const [cost, setCost] = useState(0);
  const [inputs, setInputs] = useState<RecipeSymbol[]>([]);
  const [outputs, setOutputs] = useState<RecipeSymbol[]>([]);
  const [selectedSymbols, setSelectedSymbols] = useState<SelectedSymbol[]>([]);
  const [isInputDialogOpen, setIsInputDialogOpen] = useState(false);
  const [isOutputDialogOpen, setIsOutputDialogOpen] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = async () => {
    setIsProcessing(true);
    try {
      const response = await plutusApi.createRecipe({
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

      router.replace(`/plutus/${marketId}/recipes/${response.id}`);
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
