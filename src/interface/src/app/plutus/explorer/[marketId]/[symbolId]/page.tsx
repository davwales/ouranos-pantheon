"use client";

import ClipboardCopy from "@/app/components/clipboard-copy";
import { PrettyNumber } from "@/app/components/pretty-number/pretty-number";
import { Typography } from "@/app/components/typography";
import PercentChange from "@/app/plutus/components/percent-change";
import PriceChart from "@/app/plutus/components/price-chart";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";

import { minuteSeconds } from "@/app/plutus/constants/time_frames";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { GET_SYMBOL_DETAILS } from "@/app/plutus/queries";
import useInterval from "@/hooks/use_interval";
import { useQuery } from "@urql/next";
import { useParams } from "next/navigation";
import React, { ReactNode, useMemo } from "react";

export default function SymbolDetail() {
  const { symbolId } = useParams<{ marketId: string; symbolId: string }>();
  const [timeFrameSeconds] = usePlutusStore((state: PlutusState) => [
    state.timeFrameSeconds,
  ]);

  useInterval(() => reexecuteQuery(), minuteSeconds * 1000);

  const [{ data }, reexecuteQuery] = useQuery({
    query: GET_SYMBOL_DETAILS,
    variables: {
      symbolId: symbolId,
      seconds: timeFrameSeconds > 0 ? timeFrameSeconds : undefined,
    },
  });

  const formattedTrades = useMemo(
    () =>
      data?.symbolTrades.trades.map((t) => {
        return {
          ...t,
          date: new Date(t.date),
        };
      }) ?? [],
    [data?.symbolTrades.trades]
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
        <PrettyNumber number={data?.symbolTrades.totalSpent} />
      </StatDisplay>
      <StatDisplay label="Minimum Price">
        <ClipboardCopy value={data?.symbolTrades.minPrice}>
          <PrettyNumber number={data?.symbolTrades.minPrice} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Average Price">
        <ClipboardCopy value={data?.symbolTrades.averagePrice}>
          <PrettyNumber number={data?.symbolTrades.averagePrice} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Maximum Price">
        <ClipboardCopy value={data?.symbolTrades.maxPrice}>
          <PrettyNumber number={data?.symbolTrades.maxPrice} />
        </ClipboardCopy>
      </StatDisplay>
      <StatDisplay label="Volume">
        <PrettyNumber number={data?.symbolTrades.volume} />
      </StatDisplay>
      <StatDisplay label="# Transactions">
        <PrettyNumber
          number={data?.symbolTrades.numTransactions || 0}
          decimals={0}
        />
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
    data?.allForecasts?.nodes?.[0] ? (
      <PercentChange
        label={label}
        current={current}
        previous={data.allForecasts.nodes[0].latest.averagePrice}
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
        <PriceChange
          label="Latest"
          current={data?.latestTrade?.nodes?.[0]?.price}
        />
        <PriceChange
          label="Today"
          current={data?.dailySymbolSummary.averagePrice}
        />
        <PriceChange
          label="Predicted"
          current={data?.allForecasts?.nodes?.[0]?.predictions[0].averagePrice}
        />
      </div>

      <Stats className="mt-2 gap-x-40 grid grid-cols-1 md:grid-cols-2" />
      <PriceChart data={formattedTrades} className="mt-8 max-h-96 w-full" />
    </div>
  );
}
