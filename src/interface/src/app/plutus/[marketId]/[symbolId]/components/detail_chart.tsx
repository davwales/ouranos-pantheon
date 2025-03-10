import { LineChart, SeriesConfiguration } from "@/app/components/core/charting/line_chart";
import { StyleProps } from "@/app/components/core/style_props";
import { abbreviateNumber } from "@/app/components/utils/pretty_number";
import { GetSymbolTradeBucketsResponse } from "@/gql/graphql";

interface DetailChartProps {
    trades: GetSymbolTradeBucketsResponse[];
    styling?: StyleProps;
};

export default function DetailChart(props: DetailChartProps) {
    const series: { [key: string]: SeriesConfiguration } = {
        "min_price": {
            label: "Minimum Price",
            color: "#F92069",
            isRight: false,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.minPrice
        },
        "price": {
            label: "Average Price",
            color: "#FCAA67",
            isRight: false,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.price
        },
        "max_price": {
            label: "Maximum Price",
            color: "#50BA55",
            isRight: false,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.maxPrice
        }
    };

    return (
        <LineChart
            xAxis={[
                {
                    dataKey: "date",
                    label: "Transaction Time",
                    scaleType: "time",
                    value: (trade: GetSymbolTradeBucketsResponse) => new Date(trade.date)
                }
            ]}
            yAxis={[
                {
                    id: "leftAxis",
                    scaleType: "linear",
                    valueFormatter: (x: number) => abbreviateNumber(x)
                }
            ]}
            series={series}
            dataset={props.trades}
            styling={props.styling}
        />
    );
}