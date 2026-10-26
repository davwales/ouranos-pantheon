"use client";

import { Progress } from "@/components/ui/progress";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use-interval";
import { type Market, plutusApi } from "@/lib/api/plutus";
import { FlaskConical } from "lucide-react";
import { DashboardCard } from "./dashboard-card";
import { DashboardRow } from "./dashboard-row";

const BACKTESTS_SHOWN = 5;
const REFRESH_INTERVAL_MS = 15_000;

export function ActiveBacktestsView({ markets }: { markets: Market[] }) {
  const marketNames = new Map(markets.map((m) => [m.id, m.name]));
  const [state, refresh] = useApi(
    () => plutusApi.getActiveBacktests({ take: BACKTESTS_SHOWN }),
    [],
  );

  useInterval(refresh, state.data?.totalCount ? REFRESH_INTERVAL_MS : null);

  return (
    <DashboardCard
      title={
        state.data
          ? `Running Backtests (${state.data.totalCount})`
          : "Running Backtests"
      }
      icon={FlaskConical}
      state={state}
      isEmpty={(data) => data.items.length === 0}
      emptyText="No backtests or optimizations are running."
    >
      {(data) => (
        <ul>
          {data.items.map((backtest) => (
            <DashboardRow
              key={backtest.id}
              href={`/plutus/${backtest.marketId}/strategies/${backtest.strategyId}/backtests/${backtest.id}`}
              primary={backtest.strategyName}
              secondary={[
                marketNames.get(backtest.marketId),
                backtest.kind,
                backtest.progressMessage ?? backtest.status,
              ]
                .filter(Boolean)
                .join(" · ")}
              trailing={
                <div className="flex w-24 items-center gap-2">
                  <Progress value={backtest.progressPercent} />
                  <span className="text-xs">{backtest.progressPercent}%</span>
                </div>
              }
            />
          ))}
        </ul>
      )}
    </DashboardCard>
  );
}
