"use client";

import { Button } from "@/components/ui/button";
import { Typography } from "@/components/shared/typography";
import { History, Pencil, RefreshCw, X } from "lucide-react";
import type { Recipe } from "@/lib/api/hestia-types";
import { useState } from "react";
import { AddToShoppingListButton, isShoppingListActionDisabled } from "../../_components/add-to-shopping-list-button";
import { VersionHistoryDialog } from "./version-history-dialog";

export type RecipeHeaderProps = {
  data: Recipe;
  isEditing: boolean;
  onEdit: () => void;
  onCancel: () => void;
  onReverted: () => void;
  onReimport?: () => void;
  isReimporting?: boolean;
  isInList: boolean;
  onToggleInList: () => void;
};

export function RecipeHeader({
  data,
  isEditing,
  onEdit,
  onCancel,
  onReverted,
  onReimport,
  isReimporting = false,
  isInList,
  onToggleInList,
}: RecipeHeaderProps) {
  const [historyOpen, setHistoryOpen] = useState(false);
  const reimportDisabled =
    isReimporting || data.importStatus === "Importing";
  const editDisabled =
    isReimporting ||
    data.importStatus === "Importing" ||
    data.importStatus === "Failed";

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
        <AddToShoppingListButton
          recipeId={data.id}
          isInList={isInList}
          onToggle={onToggleInList}
          disabled={
            isReimporting || isShoppingListActionDisabled(data.importStatus)
          }
        />
        <Button
          variant="outline"
          size="sm"
          className="w-full sm:w-auto"
          onClick={() => setHistoryOpen(true)}
        >
          <History className="size-4" />
          Version History
        </Button>
        {data.sourceUrl && onReimport && (
          <Button
            variant="outline"
            size="sm"
            className="w-full sm:w-auto"
            onClick={onReimport}
            disabled={reimportDisabled}
          >
            <RefreshCw className="size-4" />
            {isReimporting ? "Reimporting..." : "Reimport"}
          </Button>
        )}
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
          <Button
            size="sm"
            className="w-full sm:w-auto"
            onClick={onEdit}
            disabled={editDisabled}
          >
            <Pencil className="size-4" />
            Edit
          </Button>
        )}
      </div>
    </div>
    {historyOpen ? (
      <VersionHistoryDialog
        recipeId={data.id}
        open={historyOpen}
        onOpenChange={setHistoryOpen}
        onReverted={onReverted}
      />
    ) : null}
    </>
  );
}
