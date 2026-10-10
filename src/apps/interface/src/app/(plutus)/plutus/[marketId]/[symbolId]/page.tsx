"use client";

import { Typography } from "@/components/shared/typography";
import CandlestickChart from "@/app/(plutus)/plutus/components/candlestick-chart";
import PriceChart from "@/app/(plutus)/plutus/components/price-chart";
import TimeFrameSelection from "@/app/(plutus)/plutus/components/time-frame-selection";
import { ForecastEfficacyView } from "./_components/forecast-efficacy-view";
import PercentChange from "./_components/percent-change";
import { SignalsSection } from "./_components/signals-section";
import { SymbolAnalystButton } from "./_components/symbol-analyst-button";
import { SymbolPositionsView } from "./_components/symbol-positions-view";
import { SymbolStatsGrid } from "./_components/symbol-stats-grid";

import { PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { useNavBarActions } from "@/components/shared/nav-bar-actions-context";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use-interval";
import {
  GetDailySymbolSummaryResponse,
  GetMarketForecastRow,
  GetSymbolTradesResponse,
  plutusApi,
  Symbol,
} from "@/lib/api/plutus";
import { useParams } from "next/navigation";
import { ReactNode, useEffect, useMemo } from "react";
import { useShallow } from "zustand/react/shallow";
import { SymbolDetailSkeleton } from "@/app/(plutus)/plutus/[marketId]/[symbolId]/_components/symbol-detail-skeleton";
import { NotFoundCard } from "@/components/shared/not-found-card";

interface SymbolDetails {
  symbol: Symbol;
  trades: GetSymbolTradesResponse;
  summary: GetDailySymbolSummaryResponse;
  latestTrade?: { price: number; volume: number };
  forecast?: GetMarketForecastRow;
}

function PriceChange({
  label,
  current,
  forecast,
}: {
  label: string;
  current?: number;
  forecast?: SymbolDetails["forecast"];
}): ReactNode {
  if (!forecast) return null;
  return (
    <PercentChange
      label={label}
      current={current}
      previous={forecast.latest.averagePrice}
    />
  );
}

export default function SymbolDetail() {
  const { marketId, symbolId } = useParams<{
    marketId: string;
    symbolId: string;
  }>();
  const [timeFrameKey] = usePlutusStore(
    useShallow((state: PlutusState) => [state.timeFrameKey]),
  );

  const [state, reexecuteQuery] = useApi<SymbolDetails>(
    () =>
      Promise.all([
        plutusApi.getSymbol(symbolId),
        plutusApi.getSymbolTrades(symbolId, timeFrameKey),
        plutusApi.getDailySymbolSummary(symbolId),
        plutusApi.getAllTrades(
          "OneYear",
          {
            filter: [`symbolId:eq:${symbolId}`],
            skip: 0,
            take: 1,
            sortField: "timestamp",
            sortDirection: "desc",
          },
        ),
        plutusApi.getMarketForecasts(marketId, {
          filter: [`symbolId:eq:${symbolId}`],
          skip: 0,
          take: 1,
        }),
      ]).then(([symbol, trades, summary, allTrades, forecasts]) => ({
        symbol,
        trades,
        summary,
        latestTrade: allTrades[0],
        forecast: forecasts.items[0],
      })),
    [symbolId, timeFrameKey],
  );

  useInterval(() => reexecuteQuery(), 60000);

  const data = state.data;
  useBreadcrumbLabel(symbolId, data?.symbol.name, state.status === "error");

  const formattedTrades = useMemo(
    () =>
      state.data?.trades.trades.map((t) => {
        return {
          ...t,
          date: new Date(t.date),
        };
      }) ?? [],
    [state.data?.trades.trades],
  );

  const { setActions, clearActions } = useNavBarActions();
  const isAssistantAvailable = data?.symbol !== undefined;

  useEffect(() => {
    if (!isAssistantAvailable) {
      return;
    }

    setActions(
      <SymbolAnalystButton
        key={symbolId}
        symbolId={symbolId}
        timeFrame={timeFrameKey}
      />,
    );
    return () => clearActions();
  }, [isAssistantAvailable, symbolId, timeFrameKey, setActions, clearActions]);

  if (state.status === "error" && !data) {
    return <NotFoundCard title="Symbol not found" backHref={`/plutus/${marketId}`} backLabel="Back to Market" />;
  }

  if (state.status === "loading" && !data) {
    return <SymbolDetailSkeleton />;
  }

  return (
    <div>
      <div className="md:flex md:justify-between md:items-center">
        <Typography variant="h2" className="mb-2 border-b-0">
          {data?.symbol.name}
        </Typography>

        <TimeFrameSelection triggerClassName="w-full md:w-50" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-8 gap-2 mt-4">
        <PriceChange
          label="Latest"
          current={data?.latestTrade?.price}
          forecast={data?.forecast}
        />
        <PriceChange
          label="Today"
          current={data?.summary.averagePrice}
          forecast={data?.forecast}
        />
        <PriceChange
          label="Predicted"
          current={data?.forecast?.dayOne?.averagePrice}
          forecast={data?.forecast}
        />
      </div>

      <SymbolStatsGrid
        symbol={data?.symbol}
        trades={data?.trades}
        className="mt-2 gap-x-40 grid grid-cols-1 md:grid-cols-2"
      />
      <PriceChart data={formattedTrades} className="mt-8 max-h-96 w-full" />
      <CandlestickChart
        data={formattedTrades}
        className="mt-4 max-h-96 w-full"
      />
      <SignalsSection symbolId={symbolId} />
      <ForecastEfficacyView symbolId={symbolId} />
      <SymbolPositionsView marketId={marketId} symbol={data?.symbol} />
    </div>
  );
}
