"use client";

import { Minus, Plus } from "lucide-react";
import { useState } from "react";
import type { Ingredient } from "@/lib/api/hestia-types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  SCALE_MAX,
  SCALE_MIN,
  SCALE_STEP,
  clampScale,
  formatQuantity,
  scaleQuantity,
} from "./scale-ingredients";

export type IngredientsViewProps = {
  ingredients: Ingredient[];
};

export function IngredientsView({ ingredients }: IngredientsViewProps) {
  const [scale, setScale] = useState(1);

  const decreaseScale = () => setScale(clampScale(scale - SCALE_STEP));
  const increaseScale = () => setScale(clampScale(scale + SCALE_STEP));

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Ingredients</CardTitle>
        <div className="flex items-center gap-1">
          <Button
            variant="outline"
            size="icon-xs"
            aria-label="Decrease scale"
            onClick={decreaseScale}
            disabled={scale <= SCALE_MIN}
          >
            <Minus className="size-3" />
          </Button>
          <span
            className="min-w-10 text-center text-sm font-medium tabular-nums"
            aria-live="polite"
          >
            {`${scale}x`}
          </span>
          <Button
            variant="outline"
            size="icon-xs"
            aria-label="Increase scale"
            onClick={increaseScale}
            disabled={scale >= SCALE_MAX}
          >
            <Plus className="size-3" />
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        <ul
          role="list"
          className="grid grid-cols-[auto_auto_1fr] gap-x-3 gap-y-2 text-base">
          {ingredients.map((ingredient, index) => {
            const quantity =
              ingredient.quantity > 0
                ? formatQuantity(scaleQuantity(ingredient.quantity, scale))
                : "";

            return (
              <li key={index} className="contents">
                <span className="text-right font-mono text-lg tabular-nums text-muted-foreground">
                  {quantity}
                </span>
                <span className="text-muted-foreground">{ingredient.unit}</span>
                <span>{ingredient.name}</span>
              </li>
            );
          })}
        </ul>
      </CardContent>
    </Card>
  );
}