import { abbreviateNumber } from "@/app/components/pretty-number";
import { ChartConfig, ChartContainer, ChartLegend, ChartLegendContent, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart";
import { formatDistance } from "date-fns";
import { CartesianGrid, Line, LineChart, XAxis, YAxis } from "recharts";
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
};

export default function PriceChart({
    data,
    ...props
}: React.ComponentProps<"div"> & {
    data: {
        minPrice: number;
        price: number;
        maxPrice: number;
        date: Date;
    }[]
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

    return (
        <div {...props}>
            <ChartContainer config={chartConfig}>
                <LineChart accessibilityLayer data={data}>
                    <CartesianGrid vertical={false} />
                    <ChartLegend content={<ChartLegendContent />} />
                    <ChartTooltip
                        cursor={false}
                        content={<ChartTooltipContent hideLabel valueFormatter={tooltipValueFormatter} />}
                    />
                    <XAxis
                        dataKey="date"
                        scale="time"
                        tickLine={true}
                        axisLine={true}
                        tickMargin={8}
                        tickFormatter={dateFormatter}
                    />
                    <YAxis
                        type="number"
                        hide={false}
                        tickFormatter={(x) => abbreviateNumber(x)}
                        domain={["auto", "auto"]}
                    />
                    <Line
                        dataKey="maxPrice"
                        type="monotone"
                        stroke="var(--color-maxPrice)"
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
                        dataKey="minPrice"
                        type="monotone"
                        stroke="var(--color-minPrice)"
                        strokeWidth={2}
                        dot={false}
                    />
                </LineChart>
            </ChartContainer>
        </div>
    );
}