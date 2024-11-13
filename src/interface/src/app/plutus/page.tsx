"use client";

import { CardContent, Grid2, Typography } from "@mui/material";
import { getAllMarketsQuery } from "./queries";
import { useQuery } from "@urql/next";
import LinkCard from "../components/link_card";

export default function Plutus() {
    const [{ data }] = useQuery({ query: getAllMarketsQuery })

    return (
        <>
            <Grid2 container spacing={2}>
                {data?.allMarkets?.nodes?.map(market => (
                    <Grid2 key={market.id} size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
                        <LinkCard href={`/plutus/${market.id}`}>
                            <CardContent>
                                <Typography variant="h4" sx={{ textAlign: "center" }}>
                                    {market.name}
                                </Typography>
                            </CardContent>
                        </LinkCard>
                    </Grid2>
                ))}
            </Grid2>
        </>
    );
}