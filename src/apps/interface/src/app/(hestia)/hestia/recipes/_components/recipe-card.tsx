import { AddToShoppingListButton, isShoppingListActionDisabled } from "./add-to-shopping-list-button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import Link from "next/link";
import type { RecipeSummary } from "@/lib/api/hestia-types";

export type RecipeCardProps = {
  recipe: RecipeSummary;
  isInList: boolean;
  onToggle: (recipeId: string) => void;
};

function pluralize(count: number, noun: string): string {
  return `${count} ${noun}${count === 1 ? "" : "s"}`;
}

function formatDate(isoDate: string): string {
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  }).format(new Date(isoDate));
}

function getHostname(sourceUrl: string | null): string | null {
  if (!sourceUrl) {
    return null;
  }
  try {
    return new URL(sourceUrl).hostname.replace(/^www\./, "");
  } catch {
    return null;
  }
}

function RecipeCardStatus({ recipe }: { recipe: RecipeSummary }) {
  if (recipe.importStatus === "Importing") {
    return <Badge variant="secondary">Importing…</Badge>;
  }

  const hostname = getHostname(recipe.sourceUrl);

  if (recipe.importStatus === "Failed") {
    return (
      <Badge variant="destructive" className="max-w-full truncate">
        Import failed{hostname ? ` · ${hostname}` : ""}
      </Badge>
    );
  }

  if (recipe.importStatus === "Imported") {
    return (
      <Badge variant="outline" className="max-w-full truncate">
        Imported{hostname ? ` · ${hostname}` : ""}
      </Badge>
    );
  }

  return null;
}

function hasImportedContent(recipe: RecipeSummary): boolean {
  return (
    recipe.importStatus === "Imported" || recipe.importStatus === "None"
  );
}

export function RecipeCard({ recipe, isInList, onToggle }: RecipeCardProps) {
  const isActionDisabled = isShoppingListActionDisabled(recipe.importStatus);

  return (
    <Card className="relative h-full gap-4 py-4 transition-all hover:bg-accent hover:shadow-md has-[:focus-visible]:ring-2 has-[:focus-visible]:ring-ring">
      <CardHeader className="gap-1.5">
        <CardTitle className="line-clamp-2 text-base">
          <Link
            href={`/hestia/recipes/${recipe.id}`}
            className="after:absolute after:inset-0 after:content-[''] hover:underline underline-offset-4 focus-visible:outline-none"
          >
            {recipe.title}
          </Link>
        </CardTitle>
        <RecipeCardStatus recipe={recipe} />
      </CardHeader>
      <CardContent>
        {hasImportedContent(recipe) && (
          <div className="flex flex-col gap-1 text-sm text-muted-foreground">
            <span>
              {pluralize(recipe.ingredientCount, "ingredient")} ·{" "}
              {pluralize(recipe.stepCount, "step")}
            </span>
            <span>Added {formatDate(recipe.createdAt)}</span>
          </div>
        )}
      </CardContent>
      <CardFooter className="relative z-10 mt-auto border-t pt-4">
        <AddToShoppingListButton
          recipeId={recipe.id}
          isInList={isInList}
          onToggle={onToggle}
          disabled={isActionDisabled}
        />
      </CardFooter>
    </Card>
  );
}