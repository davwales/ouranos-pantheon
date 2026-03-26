"use client";

import { Typography } from "@/app/components/typography";
import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import { GetSymbolSignalsResponse, plutusApi } from "@/lib/api/plutus";
import { useState } from "react";
import { SignalCard } from "./signal-card";

const INTENTS = ["All", "Buy", "Sell", "Flip", "Merch"] as const;
type IntentFilter = (typeof INTENTS)[number];

function SummaryBar({ value }: { value: number }) {
  const isPositive = value > 0;
  const fillStart = isPositive ? 50 : ((value + 1) / 2) * 100;
  const fillWidth = (Math.abs(value) / 2) * 100;
  const fillColor = isPositive
    ? "bg-green-500"
    : value < 0
      ? "bg-red-500"
      : "bg-muted-foreground";

  return (
    <div className="relative h-3 bg-muted rounded-full overflow-hidden">
      <div className="absolute inset-y-0 left-1/2 w-px bg-border z-10" />
      <div
        className={`absolute inset-y-0 rounded-full ${fillColor}`}
        style={{ left: `${fillStart}%`, width: `${fillWidth}%` }}
      />
    </div>
  );
}

function SignalSummarySection({
  data,
}: {
  data: GetSymbolSignalsResponse["summary"];
}) {
  const scoreColor =
    data.aggregatedScore > 0
      ? "text-green-600"
      : data.aggregatedScore < 0
        ? "text-red-500"
        : "text-muted-foreground";

  return (
    <div className="border rounded-lg p-4 mt-4 flex flex-col gap-3">
      <div className="flex items-center justify-between gap-4">
        <div>
          <Typography variant="h4">Aggregated Score</Typography>
          <Typography variant="muted" className="text-sm">
            Average signal value across all indicators
          </Typography>
        </div>
        <Typography
          variant="large"
          className={`font-mono font-bold tabular-nums ${scoreColor}`}
        >
          {data.aggregatedScore >= 0 ? "+" : ""}
          {data.aggregatedScore.toFixed(2)}
        </Typography>
      </div>

      <SummaryBar value={data.aggregatedScore} />

      <div className="flex flex-wrap gap-4 pt-1">
        <div className="flex items-center gap-2">
          <span className="h-2 w-2 rounded-full bg-green-500" />
          <Typography variant="small">{data.bullishCount} Bullish</Typography>
        </div>
        <div className="flex items-center gap-2">
          <span className="h-2 w-2 rounded-full bg-red-500" />
          <Typography variant="small">{data.bearishCount} Bearish</Typography>
        </div>
        <div className="flex items-center gap-2">
          <span className="h-2 w-2 rounded-full bg-muted-foreground" />
          <Typography variant="small">{data.neutralCount} Neutral</Typography>
        </div>
        {data.isFlipFavourable && (
          <span className="text-xs px-2 py-0.5 rounded-full bg-green-500/10 text-green-600 border border-green-500/20">
            Flip Favourable
          </span>
        )}
        {data.isMerchFavourable && (
          <span className="text-xs px-2 py-0.5 rounded-full bg-blue-500/10 text-blue-600 border border-blue-500/20">
            Merch Favourable
          </span>
        )}
      </div>
    </div>
  );
}

export function SignalsSection({ symbolId }: { symbolId: string }) {
  const [intent, setIntent] = useState<IntentFilter>("All");

  const [state] = useApi<GetSymbolSignalsResponse>(
    () =>
      plutusApi.getSymbolSignals(
        symbolId,
        intent === "All" ? undefined : intent,
      ),
    [symbolId, intent],
  );

  const data = state.data;

  return (
    <div className="mt-8">
      <Typography variant="h2">Signals</Typography>

      {data?.summary && <SignalSummarySection data={data.summary} />}

      <div className="flex gap-2 mt-4 flex-wrap">
        {INTENTS.map((i) => (
          <Button
            key={i}
            variant={intent === i ? "default" : "outline"}
            size="sm"
            onClick={() => setIntent(i)}
          >
            {i}
          </Button>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
        {data?.signals.map((signal) => (
          <SignalCard key={signal.type} signal={signal} />
        ))}
      </div>
    </div>
  );
}
