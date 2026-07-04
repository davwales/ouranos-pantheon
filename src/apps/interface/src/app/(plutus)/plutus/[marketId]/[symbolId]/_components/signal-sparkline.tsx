"use client";

import { format } from "date-fns";
import { useId } from "react";
import {
  Line,
  LineChart,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
} from "recharts";

type SignalHistoryPoint = { value: number; computedAt: string };

type SignalSparklineProps = {
  history: SignalHistoryPoint[];
};

export function SignalSparkline({ history }: SignalSparklineProps) {
  if (history.length < 2) {
    return null;
  }

  const sorted = [...history].sort(
    (a, b) => new Date(a.computedAt).getTime() - new Date(b.computedAt).getTime(),
  );

  return <SparklineChart data={sorted} />;
}

function SparklineChart({ data }: { data: SignalHistoryPoint[] }) {
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
            gradientUnits="objectBoundingBox"
            x1="0"
            x2="0"
            y1="0"
            y2="1"
          >
            <stop offset="0%" stopColor="#22c55e" />
            <stop offset="50%" stopColor="var(--muted-foreground)" />
            <stop offset="100%" stopColor="#ef4444" />
          </linearGradient>
        </defs>
        <ReferenceLine y={0} stroke="var(--border)" strokeWidth={1} />
        <Tooltip content={<SparklineTooltip />} />
        <Line
          dataKey="value"
          type="monotone"
          dot={(props) => <SparklineDot {...props} lastIndex={lastIndex} />}
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
}: {
  cx?: number;
  cy?: number;
  index?: number;
  payload?: SignalHistoryPoint;
  lastIndex: number;
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
      ? "#22c55e"
      : payload.value < 0
        ? "#ef4444"
        : "var(--muted-foreground)";

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
}: {
  active?: boolean;
  payload?: { payload: SignalHistoryPoint }[];
}) {
  if (!active || !payload?.length) {
    return null;
  }

  const point = payload[0].payload;
  const date = new Date(point.computedAt);
  const valueColor =
    point.value > 0
      ? "#22c55e"
      : point.value < 0
        ? "#ef4444"
        : "var(--muted-foreground)";

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
