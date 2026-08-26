"use client";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { RecipeHistoryEvent } from "@/lib/api/hestia-types";
import { RotateCcw } from "lucide-react";
import { useState } from "react";
import { recipeHistoryEventLabel } from "./recipe-history-constants";

type RecipeHistoryEventRowProps = {
  event: RecipeHistoryEvent;
  isLatest: boolean;
  reverting: boolean;
  onRevert: (version: number) => void;
};

export function RecipeHistoryEventRow({
  event,
  isLatest,
  reverting,
  onRevert,
}: RecipeHistoryEventRowProps) {
  const [confirming, setConfirming] = useState(false);

  return (
    <li className="rounded-lg border p-3">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0">
          <div className="flex items-center gap-2">
            <Badge variant="secondary" className="shrink-0">
              v{event.version}
            </Badge>
            <span className="font-medium">
              {recipeHistoryEventLabel(event.eventType)}
            </span>
          </div>
          <time className="mt-0.5 block text-xs text-muted-foreground">
            {new Date(event.timestamp).toLocaleString()}
          </time>
        </div>
        {isLatest ? (
          <div className="flex items-center justify-end">
            <Badge>Current</Badge>
          </div>
        ) : confirming ? (
          <div className="flex flex-col gap-2 sm:items-end">
            <p className="text-sm">Restore the recipe to v{event.version}?</p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                className="flex-1 sm:flex-none"
                onClick={() => setConfirming(false)}
                disabled={reverting}
              >
                Cancel
              </Button>
              <Button
                variant="destructive"
                size="sm"
                className="flex-1 sm:flex-none"
                onClick={() => onRevert(event.version)}
                disabled={reverting}
              >
                Revert
              </Button>
            </div>
          </div>
        ) : (
          <Button
            variant="outline"
            size="sm"
            className="w-full sm:w-auto"
            onClick={() => setConfirming(true)}
            disabled={reverting}
          >
            <RotateCcw />
            Revert
          </Button>
        )}
      </div>
    </li>
  );
}
