import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { LineSeriesType, LineChart as MuiLineChart } from "@mui/x-charts";

type ScaleType = 'linear' | 'time';
type DataType = string | number | Date;

interface XAxisConfiguration {
    dataKey: string;
    label: string;
    scaleType: ScaleType;
    value: (x: any) => DataType;
}

interface YAxisConfiguration {
    id: string;
    scaleType: ScaleType;
    valueFormatter?: (x: any) => any;
}

export interface SeriesConfiguration {
    label: string;
    color: string;
    isRight: boolean;
    formatter: (x: number | null) => string;
    value: (x: any) => DataType;
}

interface LineChartProps {
    xAxis: XAxisConfiguration[];
    yAxis: YAxisConfiguration[];
    series: { [key: string]: SeriesConfiguration };
    dataset: { [key: string]: DataType }[];
    height?: number;
    styling?: StyleProps;
}

export function LineChart(props: LineChartProps) {
    const colors: string[] = Object.entries(props.series).map(([_, value]) => value.color);

    const series: LineSeriesType[] = Object.entries(props.series).map(([key, value]) => ({
        yAxisId: value.isRight ? "rightAxis" : "leftAxis",
        dataKey: key,
        label: value.label,
        curve: "linear",
        type: "line",
        valueFormatter: value.formatter
    }));

    const dataset: { [key: string]: DataType }[] = props.dataset.map(t => {
        const value: { [key: string]: DataType } = {};

        for (let axisConfig of props.xAxis) {
            value[axisConfig.dataKey] = axisConfig.value(t);
        }

        for (let [key, seriesConfig] of Object.entries(props.series)) {
            value[key] = seriesConfig.value(t);
        }

        return value;
    });

    return (
        <MuiLineChart
            xAxis={props.xAxis}
            yAxis={props.yAxis}
            series={series}
            dataset={dataset}
            colors={colors}
            height={props.height ?? 600}
            margin={{ left: 75, right: 75 }}
            sx={{
                ...(props.styling && convertToSx(props.styling)),
                '.MuiMarkElement-root': {
                    scale: '0.4',
                    strokeWidth: 2,
                },
            }}
        />
    );
}
