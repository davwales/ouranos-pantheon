"use client";

import { PrettyNumber } from "@/app/components/pretty_number";
import useInterval from "@/app/utilities/use_interval";
import { Box, Button, Grid2, Typography } from "@mui/material";
import { useQuery } from "@urql/next";
import { useParams, useRouter } from "next/navigation";
import TimeFrameSelection from "../../components/time_frame_selection";
import { PlutusState, usePlutusStore } from "../../constants/plutus_store";
import { GET_SYMBOL_DETAILS } from "../../queries";
import DetailChart from "./components/detail_chart";

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
        "Tax": <PrettyNumber number={data?.symbolTrades.tax} />,
        //"Total Volume": <PrettyNumber number={data?.symbolTrades.} />,
        "Transactions": <PrettyNumber number={data?.symbolTrades.numTransactions || 0} decimals={0} />,
        "Margin": <PrettyNumber number={data?.symbolTrades.margin} />,
        "Gain": <PrettyNumber number={data?.symbolTrades.totalGain} />,
    };

    return (
        <>
            <Box sx={{ width: "100%", m: "auto" }}>
                <Button variant="outlined" onClick={handleBackClicked}>Back</Button>
                <TimeFrameSelection onChange={handleTimeFrameChange} seconds={timeFrameSeconds} sx={{ float: "right" }} />
            </Box>
            <Grid2 container spacing={2} sx={{ m: "auto" }}>
                <Grid2 container spacing={2} sx={{ maxWidth: "100rem", m: "auto" }}>
                    <Grid2 size={12} sx={{ textAlign: "center" }}>
                        <Typography variant="h4">{data?.symbol.name}</Typography>
                    </Grid2>
                    {Object.entries(fieldMapping).map(([fieldKey, fieldValue]) => (
                        <Grid2 key={fieldKey} size={{ sm: 12, md: 4 }} sx={{ textAlign: "center" }}>
                            <Typography variant="h6">{fieldKey}</Typography>
                            {fieldValue}
                        </Grid2>
                    ))}
                </Grid2>
            </Grid2>
            {data?.symbolTrades?.trades.length &&
                <Grid2 size={12} sx={{ textAlign: "center" }}>
                    <DetailChart trades={data.symbolTrades.trades} />
                </Grid2>
            }
        </>
    );
}