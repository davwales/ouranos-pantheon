"use client";

import { Typography } from "@/components/shared/typography";
import { RecipeForm } from "../_components/recipe-form";

export default function CreateRecipePage() {
  return (
    <div className="m-4 mx-auto max-w-5xl space-y-4">
      <Typography variant="h2" className="border-b-0">
        Create New Recipe
      </Typography>

      <RecipeForm />
    </div>
  );
}
