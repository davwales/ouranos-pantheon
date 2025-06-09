import { abbreviateNumber } from "@/app/components/pretty-number";
import {
  ChartConfig,
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { formatDistance } from "date-fns";
import { useMemo } from "react";
import {
  Bar,
  BarProps,
  CartesianGrid,
  ComposedChart,
  Line,
  XAxis,
  YAxis,
} from "recharts";
import { ValueType } from "recharts/types/component/DefaultTooltipContent";

const chartConfig: ChartConfig = {
  maxPrice: {
    label: "Maximum Price",
    color: "var(--chart-2)",
  },
  price: {
    label: "Average Price",
    color: "var(--chart-3)",
  },
  minPrice: {
    label: "Minimum Price",
    color: "var(--chart-5)",
  },
  volume: {
    label: "Volume",
    color: "var(--chart-4)",
  },
};

export default function PriceChart({
  data,
  volumePercent = 0.25,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  data: {
    minPrice: number;
    price: number;
    maxPrice: number;
    volume: number;
    date: Date;
  }[];
  volumePercent?: number;
}) {
  const dateFormatter = (date: Date): string => {
    return formatDistance(date, new Date(), { addSuffix: true });
  };

  const tooltipValueFormatter = (value: ValueType) => {
    const innerValue = value.valueOf();
    if (typeof innerValue !== "number") {
      return value;
    }

    const parsedValue = Number(innerValue);
    return abbreviateNumber(parsedValue);
  };

  if (!data || data.length < 2) {
    return <div {...props}>Not enough data to visualize.</div>;
  }

  const barShape = (opacity: number, props: BarProps) => (
    <rect
      x={props.x}
      y={props.y}
      width={Math.max(1, props.width || 0)}
      height={props.height}
      fill={props.fill}
      opacity={opacity}
    />
  );

  const chartData = useMemo(
    () =>
      data.map((d) => ({
        ...d,
        date: d.date.getTime(),
      })),
    [data]
  );

  return (
    <ChartContainer
      config={chartConfig}
      className={`min-h-[300px] ${className}`}
      {...props}
    >
      <ComposedChart accessibilityLayer data={chartData}>
        <CartesianGrid vertical={false} />
        <ChartLegend content={<ChartLegendContent />} />
        <ChartTooltip
          cursor={false}
          content={
            <ChartTooltipContent
              hideLabel
              valueFormatter={tooltipValueFormatter}
            />
          }
        />
        <XAxis
          dataKey="date"
          type="number"
          scale="time"
          domain={["auto", "auto"]}
          tickLine={true}
          axisLine={true}
          tickMargin={8}
          tickFormatter={(timestamp) => dateFormatter(new Date(timestamp))}
          padding="gap"
        />
        <YAxis
          type="number"
          hide={false}
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
          activeBar={(props: BarProps) => barShape(1, props)}
          shape={(props: BarProps) => barShape(0.5, props)}
        />
        <Line
          dataKey="minPrice"
          type="monotone"
          stroke="var(--color-minPrice)"
          strokeWidth={2}
          dot={false}
        />
        <Line
          dataKey="price"
          type="monotone"
          stroke="var(--color-price)"
          strokeWidth={2}
          dot={false}
        />
        <Line
          dataKey="maxPrice"
          type="monotone"
          stroke="var(--color-maxPrice)"
          strokeWidth={2}
          dot={false}
        />
      </ComposedChart>
    </ChartContainer>
  );
}
