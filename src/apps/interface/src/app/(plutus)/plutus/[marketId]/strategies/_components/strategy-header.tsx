"use client";

import { ConfirmationButton } from "@/components/shared/confirmation-button";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { type StrategyDetail } from "@/lib/api/plutus";
import { Activity, Pencil, Play, Sparkles, Trash2 } from "lucide-react";
import Link from "next/link";
import { InfoChip } from "./info-chip";

export function StrategyHeader({
  data,
  marketId,
  strategyId,
  isEditing,
  toggling,
  onEdit,
  onToggleActive,
  onDelete,
  onRunBacktest,
  onOptimize,
}: {
  data: StrategyDetail;
  marketId: string;
  strategyId: string;
  isEditing: boolean;
  toggling: boolean;
  onEdit: () => void;
  onToggleActive: () => void;
  onDelete: () => void;
  onRunBacktest: () => void;
  onOptimize: () => void;
}) {
  const activeInputCount = data.inputWeights.filter(
    (w) => w.weight !== 0,
  ).length;

  return (
    <Card className="border-l-4">
      <CardContent className="pt-6 pb-6">
        <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
          <div className="space-y-3 min-w-0">
            <div className="flex items-center gap-3">
              <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary shrink-0">
                <Activity className="size-5" />
              </div>
              <div className="min-w-0">
                <h2 className="text-2xl font-semibold tracking-tight truncate">
                  {data.name}
                </h2>
                <p className="text-sm text-muted-foreground flex items-center gap-2 flex-wrap">
                  <Badge variant="secondary">
                    {activeInputCount} active input
                    {activeInputCount === 1 ? "" : "s"}
                  </Badge>
                  <span
                    className={
                      data.isActive
                        ? "text-green-600 dark:text-green-400 font-medium"
                        : "text-red-600 dark:text-red-400 font-medium"
                    }
                  >
                    {data.isActive ? "Active" : "Inactive"}
                  </span>
                </p>
              </div>
            </div>

            {data.description && (
              <p className="text-muted-foreground text-sm">
                {data.description}
              </p>
            )}

            <div className="flex flex-wrap gap-2">
              <InfoChip
                label="Created"
                value={new Date(data.createdAt).toLocaleDateString()}
              />
              <InfoChip
                label="Updated"
                value={new Date(data.updatedAt).toLocaleDateString()}
              />
              <Link
                href={`/plutus/${marketId}/strategies/${strategyId}/backtests`}
                className="inline-flex items-center gap-1 rounded-full border bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary hover:bg-primary/20 transition-colors"
              >
                View Backtests
              </Link>
            </div>
          </div>

          {!isEditing && (
            <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 w-full sm:w-auto">
              <Button
                className="w-full sm:w-auto"
                variant="ghost"
                size="sm"
                onClick={onEdit}
              >
                <Pencil className="size-4 mr-1" />
                Edit
              </Button>
              <Button
                className="w-full sm:w-auto"
                onClick={onToggleActive}
                disabled={toggling}
                variant={data.isActive ? "destructive" : "default"}
                size="sm"
              >
                {toggling
                  ? data.isActive
                    ? "Deactivating..."
                    : "Activating..."
                  : data.isActive
                    ? "Deactivate"
                    : "Activate"}
              </Button>
              <Button
                className="w-full sm:w-auto"
                variant="outline"
                size="sm"
                onClick={onRunBacktest}
              >
                <Play className="size-4 mr-1" />
                Run Backtest
              </Button>
              <Button
                className="w-full sm:w-auto"
                size="sm"
                onClick={onOptimize}
              >
                <Sparkles className="size-4 mr-1" />
                Optimize
              </Button>
              <ConfirmationButton
                className="w-full sm:w-auto"
                title="Delete Strategy"
                description="Are you sure you want to delete this strategy? This action cannot be undone."
                onConfirm={onDelete}
                variant="outline"
                size="sm"
              >
                <Trash2 className="size-4 mr-1" />
                Delete
              </ConfirmationButton>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
