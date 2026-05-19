"use client";

import { Input } from "@/components/ui/input";
import { NumericInput } from "@/components/shared/numeric-input";

export function RecipeForm({
  name,
  cost,
  onNameChange,
  onCostChange,
}: {
  name: string;
  cost: number;
  onNameChange: (value: string) => void;
  onCostChange: (value: number) => void;
}) {
  return (
    <div className="space-y-6 mt-4">
      <div>
        <label className="text-sm font-medium">Recipe Name</label>
        <Input
          value={name}
          onChange={(e) => onNameChange(e.target.value)}
          className="mt-1"
        />
      </div>

      <div>
        <NumericInput
          label="Cost"
          value={cost}
          onChange={(v) => onCostChange(v ?? 0)}
          min={0}
          step={0.01}
        />
      </div>
    </div>
  );
}
