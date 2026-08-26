"use client";

import { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Typography } from "@/components/shared/typography";
import { useApi, type ApiState } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import type { GetVolumeHeatmapResponse } from "@/lib/api/plutus-types";
import {
  HeatmapChart,
  HeatmapChartSkeleton,
  type HeatmapChartCell,
} from "@/components/shared/heatmap-chart";

const LOOKBACK_WEEKS = 4;

const lookbackDays = LOOKBACK_WEEKS * 7;

const MONDAY = new Date(2025, 0, 6); // known Monday

const DAY_LABELS = Array.from({ length: 7 }, (_, i) => {
  const d = new Date(MONDAY);
  d.setDate(MONDAY.getDate() + i);
  return new Intl.DateTimeFormat(undefined, { weekday: "short" }).format(d);
});

const HOUR_LABELS = Array.from({ length: 24 }, (_, i) => {
  const d = new Date(2025, 0, 1);
  d.setHours(i, 0, 0, 0);
  return new Intl.DateTimeFormat(undefined, { hour: "numeric" }).format(d);
});

function toHeatmapChartCells(
  rows: GetVolumeHeatmapResponse["rows"],
): HeatmapChartCell[] {
  return rows.map((r) => ({
    x: r.dayOfWeek,
    y: r.hour,
    value: r.totalTrades,
    percentage: r.percentage,
  }));
}

function getCurrentUtcHighlight() {
  const now = new Date();
  return {
    x: now.getUTCDay() === 0 ? 6 : now.getUTCDay() - 1,
    y: now.getUTCHours(),
  };
}

function HeatmapContent({
  state,
  reexecute,
  levels,
}: {
  state: ApiState<GetVolumeHeatmapResponse>;
  reexecute: () => void;
  levels: number;
}) {
  if ((state.status === "loading" || state.status === "idle") && !state.data) {
    return <HeatmapChartSkeleton xCount={7} yCount={24} />;
  }

  if (state.status === "error" && !state.data) {
    return (
      <div className="space-y-2">
        <Typography variant="muted" className="text-destructive">
          Failed to load volume heatmap.
        </Typography>
        <Button variant="outline" size="sm" onClick={reexecute}>
          Retry
        </Button>
      </div>
    );
  }

  if (state.data!.rows.length === 0) {
    return (
      <Typography variant="muted">
        No trade data available for the last {lookbackDays} days
      </Typography>
    );
  }

  const highlight = getCurrentUtcHighlight();
  return (
    <HeatmapChart
      data={toHeatmapChartCells(state.data!.rows)}
      xLabels={DAY_LABELS}
      yLabels={HOUR_LABELS}
      xCount={7}
      yCount={24}
      highlightX={highlight.x}
      highlightY={highlight.y}
      levels={levels}
    />
  );
}

export default function VolumeHeatmap({ marketId }: { marketId: string }) {
  const [levels, setLevels] = useState(5);
  const [heatmapState, reexecute] = useApi(
    () => plutusApi.getVolumeHeatmap(marketId, LOOKBACK_WEEKS),
    [marketId],
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Volume Heatmap</CardTitle>
        <CardDescription>
          Percentage of each day&apos;s trades - last {lookbackDays} days
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center gap-3">
          <label className="text-xs text-muted-foreground whitespace-nowrap">
            Levels: {levels}
          </label>
          <input
            type="range"
            min={2}
            max={10}
            value={levels}
            onChange={(e) => setLevels(Number(e.target.value))}
            className="h-1 w-24 accent-chart-4"
          />
        </div>
        <HeatmapContent state={heatmapState} reexecute={reexecute} levels={levels} />
      </CardContent>
    </Card>
  );
}
