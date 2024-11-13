import { abbreviateNumber } from "@/app/components/pretty_number";
import { AxisConfig, LineChart, LineSeriesType } from "@mui/x-charts";
import { Box, Checkbox, FormControlLabel, FormGroup, Grid2, SxProps } from "@mui/material";
import React, { useEffect, useState } from "react";
import ScaleSelection from "./scale_selection";
import { ChartScale } from "../models/chart_scale";
import { AxisConfiguration } from "../models/axis_configuration";
import { GetSymbolTradeBucketsResponse } from "@/gql/graphql";
import { SeriesValueFormatter } from "@mui/x-charts/internals";

interface DetailChartProps {
    trades: GetSymbolTradeBucketsResponse[],
    sx?: SxProps
    chartSx?: SxProps
};

export default function DetailChart(props: DetailChartProps) {
    const [leftScale, setLeftScale] = useState<ChartScale>("linear");
    const [rightScale, setRightScale] = useState<ChartScale>("linear");
    const [axisConfig, setAxisConfig] = useState<{ [key: string]: AxisConfiguration }>({
        "price": {
            label: "Price",
            color: "#FCAA67",
            isActive: false,
            isRight: false,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.price
        },
        "volume": {
            label: "Volume",
            color: "#CFCFEA",
            isActive: false,
            isRight: true,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.volume
        },
        "max_price": {
            label: "Maximum Price",
            color: "#50BA55",
            isActive: true,
            isRight: false,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.maxPrice
        },
        "min_price": {
            label: "Minimum Price",
            color: "#F92069",
            isActive: true,
            isRight: false,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.minPrice
        },
        "margin": {
            label: "Margin",
            color: "#56CBF9",
            isActive: false,
            isRight: true,
            formatter: (x: number | null) => x ? abbreviateNumber(x) : "",
            value: (x: GetSymbolTradeBucketsResponse) => x.margin
        }
    });

    const handleAxisToggle = (key: string) => {
        const keyExists = key in axisConfig;
        if (!keyExists) {
            return;
        }

        const newConfig = { ...axisConfig };
        newConfig[key].isActive = !newConfig[key].isActive;
        setAxisConfig(newConfig);
    };

    const getActiveAxis = () => Object.entries(axisConfig).filter(([_, value]) => value.isActive);
    const getColors = (): string[] => getActiveAxis().map(([_, value]) => value.color);

    const getSeries = (): LineSeriesType[] => getActiveAxis().map(([key, value]) => ({
        yAxisKey: value.isRight ? "rightAxis" : "leftAxis",
        dataKey: key,
        label: value.label,
        curve: "linear",
        type: "line",
        valueFormatter: value.formatter
    }));

    const hasRightAxis = Boolean(getActiveAxis().find(([_, value]) => value.isRight));

    const getDataset = (): { [key: string]: number | string | Date }[] => props.trades.map(t => {
        const tradeValue: { [key: string]: number | string | Date } = {
            date: new Date(t.date)
        };
        for (let [key, axis] of getActiveAxis()) {
            tradeValue[key] = axis.value(t);
        }
        return tradeValue;
    });

    return (
        <Box sx={{ ...props.sx }}>
            <LineChart
                xAxis={[
                    {
                        dataKey: "date",
                        label: "Transaction Time",
                        scaleType: "time"
                    }
                ]}
                yAxis={[
                    {
                        id: "leftAxis",
                        scaleType: leftScale,
                        valueFormatter: (x: number) => abbreviateNumber(x)
                    },
                    {
                        id: "rightAxis",
                        scaleType: rightScale,
                        valueFormatter: (x: number) => abbreviateNumber(x)
                    }
                ]}
                series={getSeries()}
                dataset={getDataset()}
                rightAxis={hasRightAxis ? "rightAxis" : null}
                colors={getColors()}
                height={600}
                margin={{ left: 75, right: 75 }}
                sx={{
                    ...props.chartSx,
                    '.MuiMarkElement-root': {
                        scale: '0.4',
                        strokeWidth: 2,
                    },
                }}
            />
            <Grid2 container spacing={2}>
                <Grid2 size={{ sm: 12, md: 4 }} sx={{ m: "auto" }}>
                    <FormGroup row sx={{ justifyContent: "center" }}>
                        {Object.entries(axisConfig).map(([key, config]) => (
                            <FormControlLabel key={key} control={<Checkbox onChange={() => handleAxisToggle(key)} checked={config.isActive} />} label={config.label} />
                        ))}
                    </FormGroup>
                </Grid2>
                <Grid2 size={{ sm: 12, md: 8 }} sx={{ m: "auto" }}>
                    <ScaleSelection label="Left Axis Scale" availableScales={["linear", "log", "sqrt"]} scale={leftScale} onChange={setLeftScale} sx={{ mx: "0.5rem" }} />
                    <ScaleSelection label="Right Axis Scale" availableScales={["linear", "log", "sqrt"]} scale={rightScale} onChange={setRightScale} sx={{ mx: "0.5rem" }} />
                </Grid2>
            </Grid2>
        </Box>
    );
}