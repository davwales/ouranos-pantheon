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
