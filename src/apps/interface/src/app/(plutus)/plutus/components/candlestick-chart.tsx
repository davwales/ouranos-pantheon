import { abbreviateNumber } from "@/components/shared/pretty-number";
import {
  ChartConfig,
  ChartContainer,
  ChartLegend,
  ChartTooltip,
} from "@/components/ui/chart";
import {
  format,
  formatDistance,
  formatDuration,
  intervalToDuration,
} from "date-fns";
import { useMemo } from "react";
import * as RechartsPrimitive from "recharts";
import {
  Bar,
  BarShapeProps,
  CartesianGrid,
  ComposedChart,
  XAxis,
  YAxis,
} from "recharts";
import { DataPoint } from "./chart-types";
import { processChartData } from "./chart-utils";
import { GapAxisTick } from "./gap-axis-tick";

const chartConfig: ChartConfig = {
  bullish: { label: "Bullish", color: "#22c55e" },
  bearish: { label: "Bearish", color: "#ef4444" },
};

function Tooltip({
  active,
  payload,
  gaps,
}: Partial<RechartsPrimitive.TooltipContentProps> & {
  gaps: { afterIndex: number; durationMs: number }[];
}) {
  if (!active || !payload?.length) return null;

  const point = payload[0]?.payload as
    | (DataPoint & { _chartIndex: number })
    | undefined;
  if (!point) return null;

  const gap = gaps.find((g) => g.afterIndex === point._chartIndex - 1);

  return (
    <div className="grid min-w-40 items-start gap-1.5 rounded-lg border border-border/50 bg-background px-2.5 py-1.5 text-xs shadow-xl">
      <p className="font-medium">{format(point.date, "MMM d, yyyy HH:mm")}</p>
      {gap && (
        <p className="text-muted-foreground italic">
          {formatDuration(
            intervalToDuration({ start: 0, end: gap.durationMs }),
            {
              format: ["days", "hours", "minutes"],
              zero: false,
            },
          )}{" "}
          skipped before this point
        </p>
      )}
      <div className="grid gap-1">
        {[
          { label: "Open", value: point.openPrice ?? 0 },
          { label: "High", value: point.maxPrice ?? 0 },
          { label: "Low", value: point.minPrice ?? 0 },
          { label: "Close", value: point.closePrice ?? 0 },
        ].map(({ label, value }) => (
          <div key={label} className="flex items-center justify-between gap-4">
            <span className="text-muted-foreground">{label}</span>
            <span className="font-mono font-medium tabular-nums">
              {abbreviateNumber(value)}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

function CandlestickShape({
  x,
  y,
  width,
  height,
  payload,
  active = false,
}: BarShapeProps & { payload?: DataPoint; active?: boolean }) {
  if (!payload) return null;

  const px = Number(x ?? 0);
  const py = Number(y ?? 0);
  const pw = Math.max(Number(width ?? 0), 1);
  const ph = Number(height ?? 0);
  const wickX = px + pw / 2;

  const openPrice = payload.openPrice ?? 0;
  const closePrice = payload.closePrice ?? 0;
  const minPrice = payload.minPrice ?? 0;
  const maxPrice = payload.maxPrice ?? 0;

  const bullish = closePrice >= openPrice;
  const color = bullish ? "#22c55e" : "#ef4444";

  const range = maxPrice - minPrice;
  if (range === 0) {
    return (
      <line
        x1={px}
        x2={px + pw}
        y1={py}
        y2={py}
        stroke={color}
        strokeWidth={active ? 3 : 2}
      />
    );
  }

  const openY = py + ph * (1 - (openPrice - minPrice) / range);
  const closeY = py + ph * (1 - (closePrice - minPrice) / range);
  const bodyTop = Math.min(openY, closeY);
  const bodyHeight = Math.max(Math.abs(closeY - openY), 1);

  return (
    <g>
      <line
        x1={wickX}
        x2={wickX}
        y1={py}
        y2={py + ph}
        stroke={color}
        strokeWidth={active ? 2 : 1}
      />
      <rect
        x={px}
        y={bodyTop}
        width={pw}
        height={bodyHeight}
        fill={color}
        stroke={active ? "white" : color}
        strokeWidth={active ? 1.5 : 0}
        strokeOpacity={active ? 0.6 : 0}
      />
    </g>
  );
}

export default function CandlestickChart({
  data,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  data: DataPoint[];
}) {
  const { data: processedData, gaps } = useMemo(
    () => processChartData(data),
    [data],
  );

  if (!data || data.length < 2) {
    return <div {...props}>Not enough data to visualize.</div>;
  }

  const hasGaps = gaps.length > 0;

  const dateFormatter = (value: number | Date) => {
    const date = value instanceof Date ? value : new Date(value);
    return formatDistance(date, new Date(), { addSuffix: true });
  };

  const indexTickFormatter = (index: number) => {
    const point = processedData[index];
    return point ? format(point.date, "MMM d, HH:mm") : "";
  };

  const processedWithRange = processedData.map((d) => ({
    ...d,
    priceRange: [d.minPrice ?? 0, d.maxPrice ?? 0] as [number, number],
  }));

  return (
    <ChartContainer
      config={chartConfig}
      className={`min-h-75 ${className}`}
      {...props}
    >
      <ComposedChart accessibilityLayer data={processedWithRange}>
        <CartesianGrid vertical={false} />
        <ChartLegend
          content={() => (
            <div className="flex items-center justify-center gap-4 pt-3 text-xs">
              {Object.entries(chartConfig).map(([key, cfg]) => (
                <div key={key} className="flex items-center gap-1.5">
                  <div
                    className="h-2 w-2 shrink-0 rounded-[2px]"
                    style={{ backgroundColor: cfg.color }}
                  />
                  {cfg.label}
                </div>
              ))}
            </div>
          )}
        />
        <ChartTooltip cursor={false} content={<Tooltip gaps={gaps} />} />

        {hasGaps ? (
          <XAxis
            dataKey="_chartIndex"
            type="category"
            tickLine={true}
            axisLine={true}
            tickMargin={8}
            minTickGap={60}
            interval="preserveStartEnd"
            tick={(p) => (
              <GapAxisTick
                {...p}
                gaps={gaps}
                tickFormatter={indexTickFormatter}
              />
            )}
          />
        ) : (
          <XAxis
            dataKey="date"
            type="number"
            scale="time"
            domain={["auto", "auto"]}
            tickLine={true}
            axisLine={true}
            tickMargin={8}
            tickFormatter={(ts) => dateFormatter(new Date(ts))}
            padding="gap"
          />
        )}

        <YAxis
          type="number"
          tickFormatter={(x) => abbreviateNumber(x)}
          domain={["auto", "auto"]}
        />

        <Bar
          dataKey="priceRange"
          shape={(p: BarShapeProps) => (
            <CandlestickShape {...p} payload={p.payload as DataPoint} />
          )}
          activeBar={(p: BarShapeProps) => (
            <CandlestickShape {...p} payload={p.payload as DataPoint} active />
          )}
          isAnimationActive={false}
        />
      </ComposedChart>
    </ChartContainer>
  );
}
