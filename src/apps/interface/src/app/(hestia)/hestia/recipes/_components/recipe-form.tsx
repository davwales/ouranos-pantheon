"use client";

import { Button } from "@/components/ui/button";
import { Card, CardFooter } from "@/components/ui/card";
import Link from "next/link";
import { RecipeFormFields } from "./recipe-form-fields";
import { useRecipeForm } from "./use-recipe-form";

export function RecipeForm() {
  const { submit, ...form } = useRecipeForm();

  return (
    <Card>
      <form onSubmit={(e) => { e.preventDefault(); void submit(); }}>
        <RecipeFormFields {...form} />

        <CardFooter className="mt-6 flex justify-between gap-4 border-t pt-6">
          <Button variant="outline" asChild disabled={form.isSubmitting}>
            <Link href="/hestia/recipes">Cancel</Link>
          </Button>
          <Button type="submit" disabled={form.isSubmitting}>
            Save
          </Button>
        </CardFooter>
      </form>
    </Card>
  );
}
