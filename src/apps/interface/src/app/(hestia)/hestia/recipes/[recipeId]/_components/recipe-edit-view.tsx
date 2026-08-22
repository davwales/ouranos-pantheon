"use client";

import { Button } from "@/components/ui/button";
import { Card, CardFooter } from "@/components/ui/card";
import type { Recipe } from "@/lib/api/hestia-types";
import { RecipeFormFields } from "../../_components/recipe-form-fields";
import { useRecipeEditForm } from "../../_components/use-recipe-edit-form";

export type RecipeEditViewProps = {
  data: Recipe;
  onCancel: () => void;
  onSaved: () => void;
};

export function RecipeEditView({ data, onCancel, onSaved }: RecipeEditViewProps) {
  const { submit, ...form } = useRecipeEditForm({ data, onSaved });

  return (
    <Card>
      <form onSubmit={(e) => { e.preventDefault(); void submit(); }}>
        <RecipeFormFields {...form} />

        <CardFooter className="mt-6 flex justify-between gap-4 border-t pt-6">
          <Button type="button" variant="outline" onClick={onCancel} disabled={form.isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" disabled={form.isSubmitting}>
            Save
          </Button>
        </CardFooter>
      </form>
    </Card>
  );
}
