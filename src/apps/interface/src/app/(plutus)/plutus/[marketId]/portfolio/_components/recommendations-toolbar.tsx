import { NumericInput } from "@/components/shared/numeric-input";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { type Strategy } from "@/lib/api/plutus";
import { Sparkles } from "lucide-react";

export function RecommendationsToolbar({
  strategies,
  strategyId,
  onStrategyChange,
  budget,
  onBudgetChange,
  loading,
  onGenerate,
}: {
  strategies: Strategy[];
  strategyId: string;
  onStrategyChange: (strategyId: string) => void;
  budget: number;
  onBudgetChange: (budget: number) => void;
  loading: boolean;
  onGenerate: () => void;
}) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center gap-2">
      <Select value={strategyId} onValueChange={onStrategyChange}>
        <SelectTrigger className="w-full sm:w-56" aria-label="Strategy">
          <SelectValue placeholder="Select a strategy" />
        </SelectTrigger>
        <SelectContent>
          {strategies.map((strategy) => (
            <SelectItem key={strategy.id} value={strategy.id}>
              {strategy.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <NumericInput
        aria-label="Budget"
        placeholder="Budget"
        value={budget}
        onChange={(v) => onBudgetChange(v ?? 0)}
        min={1}
        className="w-full sm:w-40"
      />
      <Button
        onClick={onGenerate}
        disabled={loading || !strategyId || budget <= 0}
        className="sm:ml-auto"
      >
        <Sparkles className="w-4 h-4 mr-1.5" />
        {loading ? "Generating..." : "Generate"}
      </Button>
    </div>
  );
}
