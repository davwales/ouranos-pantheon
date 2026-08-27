import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { Recipe } from "@/lib/api/hestia-types";
import { IngredientsView } from "./ingredients-view";

export type RecipeReadOnlyViewProps = {
  data: Recipe;
};

export function RecipeReadOnlyView({ data }: RecipeReadOnlyViewProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <IngredientsView ingredients={data.ingredients} />

      <Card>
        <CardHeader>
          <CardTitle>Instructions</CardTitle>
        </CardHeader>
        <CardContent>
          {data.steps.length > 0 ? (
            <ol className="list-decimal space-y-2 pl-6 text-base leading-relaxed marker:text-muted-foreground marker:font-medium">
              {data.steps.map((step, index) => (
                <li key={index} className="pl-1">{step.text}</li>
              ))}
            </ol>
          ) : (
            <p className="text-base text-muted-foreground">No instructions provided.</p>
          )}
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