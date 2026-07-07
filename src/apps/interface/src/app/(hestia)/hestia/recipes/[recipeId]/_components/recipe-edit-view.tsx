import { Card, CardContent } from "@/components/ui/card";
import type { Recipe } from "@/lib/api/hestia-types";

export type RecipeEditViewProps = {
  data: Recipe;
  onCancel: () => void;
  onSaved: () => void;
};

export function RecipeEditView({ data }: RecipeEditViewProps) {
  return (
    <Card>
      <CardContent className="p-6 text-sm text-muted-foreground">
        Editing for &ldquo;{data.title}&rdquo; is not yet available. Use the
        Version History panel to inspect the recipe&rsquo;s event timeline.
      </CardContent>
    </Card>
  );
}