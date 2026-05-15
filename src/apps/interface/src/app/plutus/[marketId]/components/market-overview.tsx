"use client";

import { abbreviateNumber } from "@/app/components/pretty-number";
import { Typography } from "@/app/components/typography";
import PriceChart from "@/app/plutus/components/price-chart";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use_interval";
import { plutusApi } from "@/lib/api/plutus";
import { useMemo } from "react";
import { useShallow } from "zustand/react/shallow";
import { ChartSkeleton } from "@/app/components/skeletons/chart-skeleton";
import { Skeleton } from "@/components/ui/skeleton";

export default function MarketOverview({ marketId }: { marketId: string }) {
  const timeFrameKey = usePlutusStore(
    useShallow((state: PlutusState) => state.timeFrameKey),
  );

  const [overviewState, reexecute] = useApi(
    () => plutusApi.getMarketOverview(marketId, timeFrameKey),
    [marketId, timeFrameKey],
  );

  useInterval(reexecute, 60000);

  const formattedTrades = useMemo(() => {
    if (overviewState.status !== "success") return [];
    return overviewState.data.trades.map((t) => ({
      ...t,
      date: new Date(t.date),
    }));
  }, [overviewState]);

  const overview =
    overviewState.status === "success" ? overviewState.data : null;

  if (overviewState.status === "loading" && !overview) {
    return (
      <div className="space-y-4" aria-hidden="true">
        <div className="flex items-center justify-between">
          <Skeleton className="h-7 w-36" />
          <TimeFrameSelection />
        </div>
        <div className="flex gap-6">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-4 w-28" />
        </div>
        <ChartSkeleton className="mt-2" legendCount={2} />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Typography variant="h3">Market Overview</Typography>
        <TimeFrameSelection />
      </div>

      {overview && (
        <div className="flex gap-6 text-sm text-muted-foreground">
          <span>
            Avg Price:{" "}
            <span className="font-medium text-foreground">
              {abbreviateNumber(overview.averagePrice)}
            </span>
          </span>
          <span>
            Volume:{" "}
            <span className="font-medium text-foreground">
              {abbreviateNumber(overview.volume)}
            </span>
          </span>
          <span>
            Transactions:{" "}
            <span className="font-medium text-foreground">
              {overview.numTransactions.toLocaleString()}
            </span>
          </span>
        </div>
      )}

      <PriceChart data={formattedTrades} className="mt-2 max-h-96 w-full" />
    </div>
  );
}
