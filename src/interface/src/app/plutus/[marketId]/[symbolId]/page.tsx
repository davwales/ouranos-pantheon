"use client";

import Typography from "@/app/components/core/data-display/typography";
import Button from "@/app/components/core/inputs/button";
import Box from "@/app/components/core/layout/box";
import Grid from "@/app/components/core/layout/grid";
import { PrettyNumber } from "@/app/components/utils/pretty_number";
import useInterval from "@/app/components/utils/use_interval";
import DetailChart from "@/app/plutus/[marketId]/[symbolId]/components/detail_chart";
import PercentChange from "@/app/plutus/[marketId]/[symbolId]/components/percent_change";
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
        "Minimum Price": <PrettyNumber number={data?.symbolTrades.minPrice} />,
        "Average Price": <PrettyNumber number={data?.symbolTrades.averagePrice} />,
        "Maximum Price": <PrettyNumber number={data?.symbolTrades.maxPrice} />,
        "Volume": <PrettyNumber number={data?.symbolTrades.volume} />,
        "# Transactions": <PrettyNumber number={data?.symbolTrades.numTransactions || 0} decimals={0} />,
    };

    return (
        <Box styling={{ width: '100%', p: 'medium' }}>
            <Box styling={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                mb: 'large'
            }}>
                <Box styling={{ display: 'flex', gap: 'medium', alignItems: 'center' }}>
                    <Typography variant='h3'>{data?.symbol.name}</Typography>

                    <PercentChange
                        label="Current"
                        current={data?.latestTrade?.nodes?.[0]?.price}
                        previous={data?.allForecasts?.nodes?.[0].latest?.averagePrice}
                    />

                    <PercentChange
                        label="Predicted"
                        current={data?.allForecasts?.nodes?.[0]?.predictions?.[0]?.averagePrice}
                        previous={data?.allForecasts?.nodes?.[0].latest?.averagePrice}
                    />
                </Box>

                <Box styling={{ display: 'flex', gap: 'medium' }}>
                    <Button variant="outlined" onClick={handleBackClicked}>Back</Button>
                    <TimeFrameSelection onChange={handleTimeFrameChange} seconds={timeFrameSeconds} />
                </Box>
            </Box>

            <Grid container spacing={3}>
                <Grid size={{ xs: 12, md: 4 }}>
                    <Box styling={{
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 'large'
                    }}>
                        {Object.entries(fieldMapping).map(([fieldKey, fieldValue]) => (
                            <Box key={fieldKey} styling={{
                                display: 'flex',
                                justifyContent: 'space-between',
                                alignItems: 'center',
                                width: '100%'
                            }}>
                                <Typography variant="h6">{fieldKey}</Typography>
                                {fieldValue}
                            </Box>
                        ))}
                    </Box>
                </Grid>

                <Grid size={{ xs: 12, md: 8 }}>
                    {data?.symbolTrades?.trades.length && (
                        <Box styling={{ height: '100%', minHeight: '400px' }}>
                            <DetailChart trades={data.symbolTrades.trades} />
                        </Box>
                    )}
                </Grid>
            </Grid>
        </Box>
    );
}