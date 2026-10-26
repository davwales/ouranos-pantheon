"use client";

import { abbreviateNumber } from "@/components/shared/pretty-number";
import { useApi } from "@/hooks/use-api";
import { type GetMarketTradesRow, type Market, plutusApi } from "@/lib/api/plutus";
import { cn } from "@/lib/utils";
import { TrendingUp } from "lucide-react";
import { DashboardCard } from "./dashboard-card";
import { DashboardRow } from "./dashboard-row";

const MOVERS_PER_MARKET = 3;

type MarketMovers = {
  market: Market;
  rows: GetMarketTradesRow[];
};

export function MarketMoversView({ markets }: { markets: Market[] }) {
  const [state] = useApi(
    () =>
      Promise.all(
        markets.map(async (market): Promise<MarketMovers> => {
          const page = await plutusApi.getMarketTrades(market.id, "OneDay", {
            sortField: "TotalGain",
            sortDirection: "desc",
            take: MOVERS_PER_MARKET,
          });
          return { market, rows: page.items };
        }),
      ),
    [markets.map((m) => m.id)],
  );

  return (
    <DashboardCard
      title="Top Movers (24h)"
      icon={TrendingUp}
      state={state}
      isEmpty={(data) => data.every((m) => m.rows.length === 0)}
      emptyText="No trades in the last day."
    >
      {(data) => (
        <div className="space-y-3">
          {data
            .filter((m) => m.rows.length > 0)
            .map(({ market, rows }) => (
              <div key={market.id}>
                <div className="mb-1 text-xs font-medium text-muted-foreground uppercase">
                  {market.name}
                </div>
                <ul>
                  {rows.map((row) => (
                    <DashboardRow
                      key={row.symbolId}
                      href={`/plutus/${market.id}/${row.symbolId}`}
                      primary={row.symbolName}
                      secondary={`Avg ${abbreviateNumber(row.averagePrice)} · Vol ${abbreviateNumber(row.totalVolume, 0)}`}
                      trailing={
                        <>
                          <div>{abbreviateNumber(row.totalGain)}</div>
                          <div
                            className={cn(
                              "text-xs",
                              row.roi >= 0
                                ? "text-green-600 dark:text-green-400"
                                : "text-red-600 dark:text-red-400",
                            )}
                          >
                            {(row.roi * 100).toFixed(1)}% ROI
                          </div>
                        </>
                      }
                    />
                  ))}
                </ul>
              </div>
            ))}
        </div>
      )}
    </DashboardCard>
  );
}
