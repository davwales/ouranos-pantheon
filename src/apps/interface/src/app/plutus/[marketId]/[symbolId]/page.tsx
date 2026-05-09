"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number/pretty-number";
import { Typography } from "@/app/components/typography";
import CandlestickChart from "@/app/plutus/components/candlestick-chart";
import PriceChart from "@/app/plutus/components/price-chart";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import PercentChange from "./components/percent-change";
import { SignalsSection } from "./components/signals-section";
import { SymbolPositionsView } from "./components/symbol-positions-view";

import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use_interval";
import {
  GetDailySymbolSummaryResponse,
  GetMarketForecastRow,
  GetSymbolTradesResponse,
  plutusApi,
  Symbol,
} from "@/lib/api/plutus";
import { useParams } from "next/navigation";
import React, { ReactNode, useMemo } from "react";
import { useShallow } from "zustand/react/shallow";

interface SymbolDetails {
  symbol: Symbol;
  trades: GetSymbolTradesResponse;
  summary: GetDailySymbolSummaryResponse;
  latestTrade?: { price: number; volume: number };
  forecast?: GetMarketForecastRow;
}

function StatDisplay({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}): ReactNode {
  return (
    <div className="flex justify-between items-end gap-2">
      <Typography variant="h4">{label}</Typography>
      <span className="whitespace-nowrap">{children}</span>
    </div>
  );
}

function Stats({
  data,
  ...props
}: React.ComponentProps<"div"> & { data: SymbolDetails | undefined }) {
  return (
    <div {...props}>
      <StatDisplay label="Code">{data?.symbol.code}</StatDisplay>
      {data?.symbol.subcode && (
        <StatDisplay label="Subcode">{data.symbol.subcode}</StatDisplay>
      )}
      <StatDisplay label="Total Spent">
        <PrettyNumber number={data?.trades.totalSpent ?? 0} />
      </StatDisplay>
      <StatDisplay label="Minimum Price">
        <ClipboardCopy value={data?.trades.minPrice ?? 0}>
          <PrettyNumber number={data?.trades.minPrice ?? 0} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Average Price">
        <ClipboardCopy value={data?.trades.averagePrice ?? 0}>
          <PrettyNumber number={data?.trades.averagePrice ?? 0} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Maximum Price">
        <ClipboardCopy value={data?.trades.maxPrice ?? 0}>
          <PrettyNumber number={data?.trades.maxPrice ?? 0} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Volume">
        <PrettyNumber number={data?.trades.volume ?? 0} />
      </StatDisplay>
      <StatDisplay label="# Transactions">
        <PrettyNumber number={data?.trades.numTransactions || 0} decimals={0} />
      </StatDisplay>
    </div>
  );
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
        plutusApi.getAllTrades({
          filter: [`symbolId:eq:${symbolId}`],
          skip: 0,
          take: 1,
          sortField: "timestamp",
          sortDirection: "desc",
        }),
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

      <Stats
        data={data}
        className="mt-2 gap-x-40 grid grid-cols-1 md:grid-cols-2"
      />
      <PriceChart data={formattedTrades} className="mt-8 max-h-96 w-full" />
      <CandlestickChart
        data={formattedTrades}
        className="mt-4 max-h-96 w-full"
      />
      <SignalsSection symbolId={symbolId} />
      <SymbolPositionsView marketId={marketId} symbol={data?.symbol} />
    </div>
  );
}
