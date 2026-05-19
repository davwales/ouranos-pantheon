"use client";

import { abbreviateNumber } from "@/components/shared/pretty-number/abbreviate-number";
import { Typography } from "@/components/shared/typography";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { ForecastEfficacyRow, plutusApi } from "@/lib/api/plutus";
import { subDays } from "date-fns";
import { useMemo, useState } from "react";
import { ForecastEfficacySkeleton } from "@/app/(plutus)/plutus/[marketId]/[symbolId]/_components/forecast-efficacy-skeleton";

function formatPercent(value: number | null): string {
  if (value === null) return "-";
  return `${(value * 100).toFixed(1)}%`;
}

function biasColor(bias: number | null): string {
  if (bias === null || bias === 0) return "";
  return bias > 0 ? "text-red-500" : "text-green-500";
}

function formatBias(bias: number | null): string {
  if (bias === null) return "-";
  const prefix = bias > 0 ? "+" : "";
  return `${prefix}${bias.toFixed(2)}`;
}

function Metric({
  label,
  value,
  color,
}: {
  label: string;
  value: string;
  color?: string;
}) {
  return (
    <div className="flex flex-col items-center gap-1">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className={`text-lg font-semibold tabular-nums ${color ?? ""}`}>
        {value}
      </span>
    </div>
  );
}

function ModelCard({ row }: { row: ForecastEfficacyRow }) {
  const biasLabel = row.meanBias == null
    ? "Bias"
    : row.meanBias > 0
      ? "Bias (overestimates)"
      : row.meanBias < 0
        ? "Bias (underestimates)"
        : "Bias";

  return (
    <div className="space-y-2">
      <div className="flex items-center gap-2">
        <Typography variant="small" className="font-mono font-medium">
          {row.modelName}
        </Typography>
        <Typography variant="muted" className="text-xs">
          {row.evaluatedCount} samples
        </Typography>
      </div>
      <div className="border rounded-lg bg-card p-4">
        <div className="grid grid-cols-3 gap-6">
          <Metric
            label="MAPE"
            value={formatPercent(row.meanAbsolutePercentageError)}
          />
          <Metric
            label={biasLabel}
            value={formatBias(row.meanBias)}
            color={biasColor(row.meanBias)}
          />
          <Metric
            label="MAE"
            value={
              row.meanAbsoluteError !== null
                ? abbreviateNumber(row.meanAbsoluteError, 2)
                : "-"
            }
          />
        </div>
      </div>
    </div>
  );
}

export function ForecastEfficacyView({
  symbolId,
  windowDays = 30,
}: {
  symbolId: string;
  windowDays?: number;
}) {
  const [state] = useApi(
    () =>
      plutusApi.getForecastEfficacy({
        symbolId,
        since: subDays(new Date(), windowDays).toISOString(),
        skip: 0,
        take: 50,
      }),
    [symbolId, windowDays],
  );

  const data = state.data;

  const horizons = useMemo(
    () =>
      [...new Set(data?.items.map((r) => r.horizonDays) ?? [])].sort(
        (a, b) => a - b,
      ),
    [data],
  );

  const defaultHorizon = horizons[0];
  const [selectedHorizon, setSelectedHorizon] = useState<number | null>(null);
  const activeHorizon = selectedHorizon ?? defaultHorizon;

  const filteredRows = useMemo(() => {
    if (!data?.items.length) return [];
    return data.items.filter((r) => r.horizonDays === activeHorizon);
  }, [data, activeHorizon]);

  if (state.status === "loading") {
    return <ForecastEfficacySkeleton />;
  }

  if (!data?.items.length) {
    return null;
  }

  return (
    <div className="mt-8">
      <Typography variant="h2">Forecast Accuracy</Typography>

      <div className="flex gap-2 mt-4 flex-wrap">
        {horizons.map((h) => (
          <Button
            key={h}
            variant={activeHorizon === h ? "default" : "outline"}
            size="sm"
            onClick={() => setSelectedHorizon(h)}
          >
            {h} day{h !== 1 ? "s" : ""}
          </Button>
        ))}
      </div>

      <div className="flex flex-col gap-4 mt-4">
        {filteredRows.map((row) => (
          <ModelCard key={row.modelName} row={row} />
        ))}
      </div>
    </div>
  );
}
