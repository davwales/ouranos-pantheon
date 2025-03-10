"use client";

import Typography from "@/app/components/core/data-display/typography";
import Grid from "@/app/components/core/layout/grid";
import CardContent from "@/app/components/core/surfaces/card_content";
import LinkCard from "@/app/components/surfaces/link_card";
import { GET_ALL_MARKETS } from "@/app/plutus/queries";
import { useQuery } from "@urql/next";

export default function Plutus() {
    const [{ data }] = useQuery({ query: GET_ALL_MARKETS })

    return (
        <Grid container spacing={2}>
            {data?.allMarkets?.nodes?.map(market => (
                <Grid key={market.id} size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
                    <LinkCard href={`/plutus/${market.id}`}>
                        <CardContent>
                            <Typography variant="h4" styling={{ textAlign: "center" }}>
                                {market.name}
                            </Typography>
                        </CardContent>
                    </LinkCard>
                </Grid>
            ))}
        </Grid>
    );
}