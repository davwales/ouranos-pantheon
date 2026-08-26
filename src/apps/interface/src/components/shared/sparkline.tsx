"use client";

import { format } from "date-fns";
import { useId } from "react";
import {
  Line,
  LineChart,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  YAxis,
} from "recharts";

export type SparklinePoint = { value: number; timestamp: string };

export type SparklineProps = {
  data: SparklinePoint[];
  domain?: [number, number];
  colorPositive?: string;
  colorZero?: string;
  colorNegative?: string;
};

export function Sparkline({
  data: rawData,
  domain = [-1, 1],
  colorPositive = "#22c55e",
  colorZero = "var(--muted-foreground)",
  colorNegative = "#ef4444",
}: SparklineProps) {
  if (rawData.length < 2) {
    return null;
  }

  const sorted = [...rawData].sort(
    (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime(),
  );

  return (
    <SparklineChart
      data={sorted}
      domain={domain}
      colorPositive={colorPositive}
      colorZero={colorZero}
      colorNegative={colorNegative}
    />
  );
}

type SparklineChartProps = {
  data: SparklinePoint[];
  domain: [number, number];
  colorPositive: string;
  colorZero: string;
  colorNegative: string;
};

function SparklineChart({
  data,
  domain,
  colorPositive,
  colorZero,
  colorNegative,
}: SparklineChartProps) {
  const gradientId = useId();
  const lastIndex = data.length - 1;

  return (
    <ResponsiveContainer width="100%" height={48}>
      <LineChart
        data={data}
        margin={{ top: 4, right: 2, left: 2, bottom: 4 }}
      >
        <defs>
          <linearGradient
            id={gradientId}
            gradientUnits="userSpaceOnUse"
            x1="0"
            x2="0"
            y1="4"
            y2="44"
          >
            <stop offset="0%" stopColor={colorPositive} />
            <stop offset="50%" stopColor={colorZero} />
            <stop offset="100%" stopColor={colorNegative} />
          </linearGradient>
        </defs>
        <YAxis hide domain={domain} />
        <ReferenceLine y={0} stroke="var(--border)" strokeWidth={1} />
        <Tooltip
          content={
            <SparklineTooltip
              colorPositive={colorPositive}
              colorZero={colorZero}
              colorNegative={colorNegative}
            />
          }
        />
        <Line
          dataKey="value"
          type="monotone"
          dot={(props) => (
            <SparklineDot
              {...props}
              lastIndex={lastIndex}
              colorPositive={colorPositive}
              colorZero={colorZero}
              colorNegative={colorNegative}
            />
          )}
          isAnimationActive={false}
          strokeWidth={1.5}
          stroke={`url(#${gradientId})`}
        />
      </LineChart>
    </ResponsiveContainer>
  );
}

function SparklineDot({
  cx,
  cy,
  index,
  payload,
  lastIndex,
  colorPositive,
  colorZero,
  colorNegative,
}: {
  cx?: number;
  cy?: number;
  index?: number;
  payload?: SparklinePoint;
  lastIndex: number;
  colorPositive: string;
  colorZero: string;
  colorNegative: string;
}) {
  if (
    cx === undefined ||
    cy === undefined ||
    index === undefined ||
    payload === undefined ||
    index !== lastIndex
  ) {
    return null;
  }

  const fill =
    payload.value > 0
      ? colorPositive
      : payload.value < 0
        ? colorNegative
        : colorZero;

  return (
    <circle
      cx={cx}
      cy={cy}
      r={2.5}
      fill={fill}
      stroke="var(--background)"
      strokeWidth={1.5}
    />
  );
}

function SparklineTooltip({
  active,
  payload,
  colorPositive,
  colorZero,
  colorNegative,
}: {
  active?: boolean;
  payload?: { payload: SparklinePoint }[];
  colorPositive: string;
  colorZero: string;
  colorNegative: string;
}) {
  if (!active || !payload?.length) {
    return null;
  }

  const point = payload[0].payload;
  const date = new Date(point.timestamp);
  const valueColor =
    point.value > 0
      ? colorPositive
      : point.value < 0
        ? colorNegative
        : colorZero;

  return (
    <div className="rounded-lg border border-border/50 bg-background px-2 py-1 text-xs shadow-xl">
      <div className="text-muted-foreground">{format(date, "MMM d, HH:mm")}</div>
      <div className="font-mono tabular-nums" style={{ color: valueColor }}>
        {point.value >= 0 ? "+" : ""}
        {point.value.toFixed(2)}
      </div>
    </div>
  );
}
