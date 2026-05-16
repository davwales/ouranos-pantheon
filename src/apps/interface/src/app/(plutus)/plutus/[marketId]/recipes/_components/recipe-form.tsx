import { Input } from "@/components/ui/input";

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
        <label className="text-sm font-medium">Cost</label>
        <Input
          value={cost}
          onChange={(e) => onCostChange(Number(e.target.value))}
          className="mt-1"
          type="number"
        />
      </div>
    </div>
  );
}
