"use client";

import { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import type { RecipeDetails } from "@/lib/api/hestia-types";
import { formatMinutes } from "./format-minutes";

export type RecipeDetailsViewProps = {
  details: RecipeDetails | null;
};

export function RecipeDetailsView({ details }: RecipeDetailsViewProps) {
  const [imageFailed, setImageFailed] = useState(false);

  const facts = [
    { label: "Yield", value: details?.yield },
    { label: "Prep", value: minutesText(details?.prepMinutes) },
    { label: "Cook", value: minutesText(details?.cookMinutes) },
    { label: "Total", value: minutesText(details?.totalMinutes) },
  ].filter((fact): fact is { label: string; value: string } => Boolean(fact.value));
  const imageUrl = !imageFailed ? details?.imageUrl : null;

  if (facts.length === 0 && !imageUrl) {
    return null;
  }

  return (
    <Card className="md:col-span-2">
      <CardContent className="flex flex-col gap-4 sm:flex-row sm:items-center">
        {imageUrl && (
          // Recipe images come from arbitrary sites, which next/image cannot allow-list.
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={imageUrl}
            alt=""
            referrerPolicy="no-referrer"
            onError={() => setImageFailed(true)}
            className="h-48 w-full rounded-md object-cover sm:w-72"
          />
        )}
        {facts.length > 0 && (
          <dl className="grid grid-cols-2 gap-x-8 gap-y-2 sm:grid-cols-4">
            {facts.map((fact) => (
              <div key={fact.label}>
                <dt className="text-sm text-muted-foreground">{fact.label}</dt>
                <dd className="text-base font-medium">{fact.value}</dd>
              </div>
            ))}
          </dl>
        )}
      </CardContent>
    </Card>
  );
}

function minutesText(minutes: number | null | undefined): string | null {
  return minutes ? formatMinutes(minutes) : null;
}
