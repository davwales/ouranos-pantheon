"use client";

import { Button } from "@/components/ui/button";
import { Typography } from "@/components/shared/typography";
import { History, Pencil, X } from "lucide-react";
import type { Recipe } from "@/lib/api/hestia-types";
import { useState } from "react";
import { VersionHistoryDialog } from "./version-history-dialog";

export type RecipeHeaderProps = {
  data: Recipe;
  isEditing: boolean;
  onEdit: () => void;
  onCancel: () => void;
};

export function RecipeHeader({
  data,
  isEditing,
  onEdit,
  onCancel,
}: RecipeHeaderProps) {
  const [historyOpen, setHistoryOpen] = useState(false);

  return (
    <>
    <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
      <div className="space-y-1 min-w-0">
        <Typography variant="h2" className="border-b-0">
          {data.title}
        </Typography>
        {data.sourceUrl && (
          <a
            href={data.sourceUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm text-muted-foreground underline hover:text-foreground"
          >
            View source ↗
          </a>
        )}
      </div>
      <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 w-full sm:w-auto">
        <Button
          variant="outline"
          size="sm"
          className="w-full sm:w-auto"
          onClick={() => setHistoryOpen(true)}
        >
          <History className="size-4" />
          Version History
        </Button>
        {isEditing ? (
          <Button
            variant="outline"
            size="sm"
            className="w-full sm:w-auto"
            onClick={onCancel}
          >
            <X className="size-4" />
            Cancel
          </Button>
        ) : (
          <Button size="sm" className="w-full sm:w-auto" onClick={onEdit}>
            <Pencil className="size-4" />
            Edit
          </Button>
        )}
      </div>
    </div>
    <VersionHistoryDialog
      recipeId={data.id}
      open={historyOpen}
      onOpenChange={setHistoryOpen}
    />
    </>
  );
}
