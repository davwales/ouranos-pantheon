"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number/pretty-number";
import { Typography } from "@/app/components/typography";
import PercentChange from "@/app/plutus/components/percent-change";
import PriceChart from "@/app/plutus/components/price-chart";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";

import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { useApi } from "@/hooks/use-api";
import useInterval from "@/hooks/use_interval";
import {
  GetDailySymbolSummaryResponse,
  GetSymbolTradesResponse,
  plutusApi,
  Symbol,
} from "@/lib/api/plutus";
import { useParams } from "next/navigation";
import React, { ReactNode, useMemo } from "react";

interface SymbolDetails {
  symbol: Symbol;
  trades: GetSymbolTradesResponse;
  summary: GetDailySymbolSummaryResponse;
  latestTrade?: { price: number; volume: number };
  forecast?: {
    latest: { averagePrice: number };
    predictions: Array<{ averagePrice: number }>;
  };
}

export default function SymbolDetail() {
  const { symbolId } = useParams<{ marketId: string; symbolId: string }>();
  const [timeFrameKey] = usePlutusStore((state: PlutusState) => [
    state.timeFrameKey,
  ]);

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
        plutusApi.getAllForecasts({
          filter: [`symbolId:eq:${symbolId}`],
          skip: 0,
          take: 1,
        }),
      ]).then(([symbol, trades, summary, allTrades, forecasts]) => ({
        symbol,
        trades,
        summary,
        latestTrade: allTrades[0],
        forecast: (forecasts.items as any[])[0],
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
    [data?.trades.trades],
  );

  const StatDisplay = ({
    label,
    children,
  }: {
    label: string;
    children: ReactNode;
  }): ReactNode => (
    <div className="flex justify-between items-end">
      <Typography variant="h4">{label}</Typography>
      {children}
    </div>
  );

  const Stats = (props: React.ComponentProps<"div">): ReactNode => (
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

  const PriceChange = ({
    label,
    current,
  }: {
    label: string;
    current?: number;
  }): ReactNode =>
    data?.forecast ? (
      <PercentChange
        label={label}
        current={current}
        previous={data.forecast.latest.averagePrice}
      />
    ) : null;

  return (
    <div>
      <div className="md:flex md:justify-between md:items-center">
        <Typography variant="h2" className="mb-2 border-b-0">
          {data?.symbol.name}
        </Typography>

        <TimeFrameSelection triggerClassName="w-full md:w-50" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-8 gap-2 mt-4">
        <PriceChange label="Latest" current={data?.latestTrade?.price} />
        <PriceChange label="Today" current={data?.summary.averagePrice} />
        <PriceChange
          label="Predicted"
          current={data?.forecast?.predictions[0]?.averagePrice}
        />
      </div>

      <Stats className="mt-2 gap-x-40 grid grid-cols-1 md:grid-cols-2" />
      <PriceChart data={formattedTrades} className="mt-8 max-h-96 w-full" />
    </div>
  );
}
