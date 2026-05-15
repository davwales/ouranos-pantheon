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
  Line,
  XAxis,
  YAxis,
} from "recharts";
import { DataPoint } from "./chart-types";
import { processChartData } from "./chart-utils";
import { GapAxisTick } from "./gap-axis-tick";

const chartConfigFull: ChartConfig = {
  maxPrice: { label: "Maximum Price", color: "var(--chart-2)" },
  price: { label: "Average Price", color: "var(--chart-3)" },
  minPrice: { label: "Minimum Price", color: "var(--chart-5)" },
  volume: { label: "Volume", color: "var(--chart-4)" },
};

const chartConfigAvgOnly: ChartConfig = {
  price: { label: "Average Price", color: "var(--chart-3)" },
  volume: { label: "Volume", color: "var(--chart-4)" },
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
        {payload
          .filter((item) => item.type !== "none")
          .map((item, index) => {
            const cfg = chartConfigFull[item.dataKey as string];
            const color = item.color ?? cfg?.color;
            return (
              <div
                key={`${item.dataKey ?? index}`}
                className="flex items-center justify-between gap-4"
              >
                <div className="flex items-center gap-1.5">
                  <div
                    className="h-2 w-2 shrink-0 rounded-[2px]"
                    style={{ backgroundColor: color }}
                  />
                  <span className="text-muted-foreground">
                    {cfg?.label ?? item.name}
                  </span>
                </div>
                <span className="font-mono font-medium tabular-nums">
                  {abbreviateNumber(Number(item.value))}
                </span>
              </div>
            );
          })}
      </div>
    </div>
  );
}

export default function PriceChart({
  data,
  volumePercent = 0.25,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  data: DataPoint[];
  volumePercent?: number;
}) {
  const showMinMax = data.some((d) => d.minPrice != null || d.maxPrice != null);

  const chartConfig = showMinMax ? chartConfigFull : chartConfigAvgOnly;

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

  const barShape = (opacity: number, p: BarShapeProps) => (
    <rect
      x={p.x}
      y={p.y}
      width={Math.max(1, Number(p.width) || 0)}
      height={p.height}
      fill={p.fill}
      opacity={opacity}
    />
  );

  return (
    <ChartContainer
      config={chartConfig}
      className={`min-h-75 ${className}`}
      {...props}
    >
      <ComposedChart accessibilityLayer data={processedData}>
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
        <YAxis
          yAxisId="right"
          orientation="right"
          hide={true}
          domain={[0, (dataMax: number) => dataMax * (1 / volumePercent)]}
        />

        <Bar
          yAxisId="right"
          dataKey="volume"
          fill="var(--color-volume)"
          activeBar={(p: BarShapeProps) => barShape(1, p)}
          shape={(p: BarShapeProps) => barShape(0.5, p)}
        />
        {showMinMax && (
          <Line
            dataKey="minPrice"
            type="monotone"
            stroke="var(--color-minPrice)"
            strokeWidth={2}
            dot={false}
          />
        )}
        <Line
          dataKey="price"
          type="monotone"
          stroke="var(--color-price)"
          strokeWidth={2}
          dot={false}
        />
        {showMinMax && (
          <Line
            dataKey="maxPrice"
            type="monotone"
            stroke="var(--color-maxPrice)"
            strokeWidth={2}
            dot={false}
          />
        )}
      </ComposedChart>
    </ChartContainer>
  );
}
