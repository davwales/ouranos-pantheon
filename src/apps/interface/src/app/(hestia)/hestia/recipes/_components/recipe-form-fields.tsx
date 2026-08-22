"use client";

import AutosizeTextarea from "@/components/shared/autosize-textarea";
import { Button } from "@/components/ui/button";
import { CardContent } from "@/components/ui/card";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Input } from "@/components/ui/input";
import { ChevronDown } from "lucide-react";
import { IngredientRow } from "./ingredient-row";
import { InstructionStepRow } from "./instruction-step-row";
import { RecipeFormField } from "./recipe-form-field";
import type { FormErrors, IngredientFormRow, RecipeFormState } from "./recipe-form-types";

export type RecipeFormFieldsProps = {
  form: RecipeFormState;
  errors: FormErrors;
  genericError: string | null;
  isSubmitting: boolean;
  updateField: (
    field: keyof Omit<RecipeFormState, "ingredients" | "steps">,
    value: string,
  ) => void;
  updateIngredient: (index: number, field: keyof IngredientFormRow, value: string) => void;
  addIngredient: () => void;
  removeIngredient: (index: number) => void;
  updateStep: (index: number, value: string) => void;
  addStep: () => void;
  removeStep: (index: number) => void;
  moveStepUp: (index: number) => void;
  moveStepDown: (index: number) => void;
};

export function RecipeFormFields({
  form,
  errors,
  genericError,
  isSubmitting,
  updateField,
  updateIngredient,
  addIngredient,
  removeIngredient,
  updateStep,
  addStep,
  removeStep,
  moveStepUp,
  moveStepDown,
}: RecipeFormFieldsProps) {
  return (
    <CardContent className="space-y-6">
      {genericError && (
        <div
          className="rounded-md border border-destructive bg-destructive/10 p-3 text-sm text-destructive"
          role="alert"
        >
          {genericError}
        </div>
      )}

      <RecipeFormField label="Title" required error={errors.title}>
        <Input
          type="text"
          value={form.title}
          onChange={(e) => updateField("title", e.target.value)}
          placeholder="e.g., Spaghetti Carbonara"
          disabled={isSubmitting}
        />
      </RecipeFormField>

      <div className="space-y-2">
        <label className="text-sm font-medium">
          Ingredients <span aria-hidden="true">*</span>
        </label>
        {form.ingredients.map((row, index) => (
          <IngredientRow
            key={index}
            index={index}
            quantity={row.quantity}
            unit={row.unit}
            name={row.name}
            onQuantityChange={(i, v) => updateIngredient(i, "quantity", v)}
            onUnitChange={(i, v) => updateIngredient(i, "unit", v)}
            onNameChange={(i, v) => updateIngredient(i, "name", v)}
            onRemove={removeIngredient}
            canRemove={form.ingredients.length > 1}
            disabled={isSubmitting}
            errors={errors.ingredients?.[index]}
          />
        ))}
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addIngredient}
          disabled={isSubmitting}
        >
          + Add ingredient
        </Button>
      </div>

      <div className="space-y-2">
        <label className="text-sm font-medium">
          Steps <span aria-hidden="true">*</span>
        </label>
        {form.steps.map((row, index) => (
          <InstructionStepRow
            key={row.id}
            index={index}
            text={row.text}
            onTextChange={updateStep}
            onMoveUp={moveStepUp}
            onMoveDown={moveStepDown}
            onRemove={removeStep}
            canMoveUp={index > 0}
            canMoveDown={index < form.steps.length - 1}
            canRemove={form.steps.length > 1}
            disabled={isSubmitting}
            error={errors.steps?.[index]?.text}
          />
        ))}
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addStep}
          disabled={isSubmitting}
        >
          + Add step
        </Button>
      </div>

      <RecipeFormField label="Notes" error={errors.notes}>
        <AutosizeTextarea
          value={form.notes}
          onChange={(e) => updateField("notes", e.target.value)}
          className="min-h-24"
          disabled={isSubmitting}
        />
      </RecipeFormField>

      <Collapsible>
        <Button variant="outline" size="sm" asChild>
          <CollapsibleTrigger className="flex items-center gap-2">
            More options
            <ChevronDown className="size-4 transition-transform duration-200 data-[state=open]:rotate-180" />
          </CollapsibleTrigger>
        </Button>
        <CollapsibleContent className="mt-4">
          <RecipeFormField label="Source URL" error={errors.sourceUrl}>
            <Input
              type="url"
              value={form.sourceUrl}
              onChange={(e) => updateField("sourceUrl", e.target.value)}
              placeholder="https://…"
              disabled={isSubmitting}
            />
          </RecipeFormField>
        </CollapsibleContent>
      </Collapsible>
    </CardContent>
  );
}
