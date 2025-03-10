"use client";

import Typography from "@/app/components/core/data-display/typography";
import Button from "@/app/components/core/inputs/button";
import Box from "@/app/components/core/layout/box";
import Grid from "@/app/components/core/layout/grid";
import { PrettyNumber } from "@/app/components/utils/pretty_number";
import useInterval from "@/app/components/utils/use_interval";
import DetailChart from "@/app/plutus/[marketId]/[symbolId]/components/detail_chart";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/constants/plutus_store";
import { GET_SYMBOL_DETAILS } from "@/app/plutus/queries";
import { useQuery } from "@urql/next";
import { useParams, useRouter } from "next/navigation";

export default function SymbolDetail() {
    const router = useRouter();
    const { marketId, symbolId } = useParams<{ marketId: string, symbolId: string }>();
    const [timeFrameSeconds, setTimeFrameSeconds] = usePlutusStore((state: PlutusState) => [state.timeFrameSeconds, state.setTimeFrameSeconds]);

    const [{ data }, reexecuteQuery] = useQuery({
        query: GET_SYMBOL_DETAILS,
        variables: {
            marketId: marketId,
            symbolId: symbolId,
            seconds: timeFrameSeconds > 0 ? timeFrameSeconds : undefined
        }
    });

    useInterval(() => reexecuteQuery(), 15000);

    const handleTimeFrameChange = (seconds: number) => {
        setTimeFrameSeconds(seconds);
    };

    const handleBackClicked = () => {
        router.push(`/plutus/${marketId}`);
    };

    const fieldMapping = {
        "Code": <Typography>{data?.symbol.code}</Typography>,
        "Subcode": <Typography>{data?.symbol.subcode}</Typography>,
        "Total Spent": <PrettyNumber number={data?.symbolTrades.totalSpent} />,
        "Average Price": <PrettyNumber number={data?.symbolTrades.averagePrice} />,
        "Minimum Price": <PrettyNumber number={data?.symbolTrades.minPrice} />,
        "Maximum Price": <PrettyNumber number={data?.symbolTrades.maxPrice} />,
        //"Total Volume": <PrettyNumber number={data?.symbolTrades.} />,
        "Transactions": <PrettyNumber number={data?.symbolTrades.numTransactions || 0} decimals={0} />,
        "Margin": <PrettyNumber number={data?.symbolTrades.margin} />,
        "Gain": <PrettyNumber number={data?.symbolTrades.totalGain} />,
    };

    return (
        <>
            <Box styling={{ width: "100%", m: "auto" }}>
                <Button variant="outlined" onClick={handleBackClicked}>Back</Button>
                <TimeFrameSelection onChange={handleTimeFrameChange} seconds={timeFrameSeconds} styling={{ float: "right" }} />
            </Box>
            <Grid container spacing={2} styling={{ m: "auto" }}>
                <Grid container spacing={2} styling={{ maxWidth: "100rem", m: "auto" }}>
                    <Grid size={12} styling={{ textAlign: "center" }}>
                        <Typography variant="h4">{data?.symbol.name}</Typography>
                    </Grid>
                    {Object.entries(fieldMapping).map(([fieldKey, fieldValue]) => (
                        <Grid key={fieldKey} size={{ sm: 12, md: 4 }} styling={{ textAlign: "center" }}>
                            <Typography variant="h6">{fieldKey}</Typography>
                            {fieldValue}
                        </Grid>
                    ))}
                </Grid>
            </Grid>
            {data?.symbolTrades?.trades.length &&
                <Grid size={12} styling={{ textAlign: "center" }}>
                    <DetailChart trades={data.symbolTrades.trades} />
                </Grid>
            }
        </>
    );
}