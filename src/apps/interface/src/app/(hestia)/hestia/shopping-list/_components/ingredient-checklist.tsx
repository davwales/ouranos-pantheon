"use client";

import type { ConsolidatedIngredient } from "@/lib/api/hestia-types";

export type IngredientChecklistProps = {
  ingredients: ConsolidatedIngredient[];
  checked: Set<string>;
  onToggle: (lineId: string) => void;
};

export function IngredientChecklist({
  ingredients,
  checked,
  onToggle,
}: IngredientChecklistProps) {
  return (
    <ul role="list" className="divide-y divide-border">
      {ingredients.map((ingredient) => {
        const isChecked = checked.has(ingredient.id);
        const display = [
          ingredient.quantity > 0 ? ingredient.quantity : "",
          ingredient.unit,
          ingredient.name,
        ]
          .filter((part) => part !== "")
          .join(" ");

        return (
          <li key={ingredient.id}>
            <label
              className={`flex items-center gap-3 py-3 px-1 cursor-pointer ${
                isChecked ? "line-through text-muted-foreground" : ""
              }`}
            >
              <input
                type="checkbox"
                className="h-5 w-5 cursor-pointer"
                checked={isChecked}
                onChange={() => onToggle(ingredient.id)}
              />
              <span className="text-base">{display}</span>
            </label>
          </li>
        );
      })}
    </ul>
  );
}
