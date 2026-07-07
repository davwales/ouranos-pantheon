import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { Recipe } from "@/lib/api/hestia-types";

export type RecipeReadOnlyViewProps = {
  data: Recipe;
};

export function RecipeReadOnlyView({ data }: RecipeReadOnlyViewProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Ingredients</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="space-y-2 text-base">
            {data.ingredients.map((ingredient, index) => (
              <li key={index} className="flex gap-2">
                <span className="font-mono tabular-nums text-muted-foreground">
                  {ingredient.quantity} {ingredient.unit}
                </span>
                <span>{ingredient.name}</span>
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Instructions</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="whitespace-pre-wrap text-base leading-relaxed">
            {data.instructions}
          </p>
        </CardContent>
      </Card>

      {data.notes && (
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>Notes</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="whitespace-pre-wrap text-base text-muted-foreground">
              {data.notes}
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}