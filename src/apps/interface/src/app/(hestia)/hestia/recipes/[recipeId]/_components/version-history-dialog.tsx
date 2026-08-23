"use client";

import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { useApi } from "@/hooks/use-api";
import { ApiError } from "@/lib/api-client";
import { hestiaApi } from "@/lib/api/hestia";
import type { RecipeHistoryResponse } from "@/lib/api/hestia-types";
import { useState } from "react";
import { RecipeHistoryEventRow } from "./recipe-history-event-row";

export type VersionHistoryDialogProps = {
  recipeId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onReverted: () => void;
};

export function VersionHistoryDialog({
  recipeId,
  open,
  onOpenChange,
  onReverted,
}: VersionHistoryDialogProps) {
  const [history, reexecuteHistory] = useApi<RecipeHistoryResponse>(
    () => hestiaApi.getRecipeHistory(recipeId),
    [recipeId],
  );
  const [reverting, setReverting] = useState(false);
  const [revertError, setRevertError] = useState<string | null>(null);
  const [timelineKey, setTimelineKey] = useState(0);

  const events = history.data?.events ?? [];
  const sortedEvents = [...events].sort((a, b) => b.version - a.version);
  const currentVersion = events.reduce((max, event) => Math.max(max, event.version), 0);

  async function handleRevert(version: number) {
    setRevertError(null);
    setReverting(true);
    try {
      await hestiaApi.revertRecipe(recipeId, { targetVersion: version });
      onReverted();
      reexecuteHistory();
      setTimelineKey((key) => key + 1);
    } catch (error) {
      if (error instanceof ApiError) {
        setRevertError(error.message);
      } else if (error instanceof Error) {
        setRevertError(error.message);
      } else {
        setRevertError("Revert failed. Please try again.");
      }
    } finally {
      setReverting(false);
    }
  }

  return (
    <ResponsiveDialog
      title="Version History"
      description="Review every change to this recipe and restore an earlier version."
      trigger={null}
      open={open}
      onOpenChange={onOpenChange}
    >
      <div className="space-y-4">
        {revertError && (
          <p role="alert" className="text-sm text-destructive">
            {revertError}
          </p>
        )}
        {history.status === "error" && !history.data ? (
          <p role="alert" className="text-sm text-destructive">
            Failed to load version history.
          </p>
        ) : history.status === "loading" && !history.data ? (
          <div className="space-y-2">
            <Skeleton className="h-16 w-full rounded-lg" />
            <Skeleton className="h-16 w-full rounded-lg" />
            <Skeleton className="h-16 w-full rounded-lg" />
          </div>
        ) : events.length === 0 ? (
          <p className="text-sm text-muted-foreground">No version history yet.</p>
        ) : (
          <ol
            key={timelineKey}
            className="max-h-[60dvh] space-y-2 overflow-y-auto pr-1"
          >
            {sortedEvents.map((event) => (
              <RecipeHistoryEventRow
                key={event.version}
                event={event}
                isLatest={event.version === currentVersion}
                reverting={reverting}
                onRevert={handleRevert}
              />
            ))}
          </ol>
        )}
      </div>
    </ResponsiveDialog>
  );
}
