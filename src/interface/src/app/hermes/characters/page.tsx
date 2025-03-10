"use client";

import Typography from "@/app/components/core/data-display/typography";
import Box from "@/app/components/core/layout/box";
import Grid from "@/app/components/core/layout/grid";
import CardContent from "@/app/components/core/surfaces/card_content";
import LinkCard from "@/app/components/surfaces/link_card";
import { GET_CHARACTER_LIST } from "@/app/hermes/queries";
import { useQuery } from "@urql/next";

export default function CharactersPage() {
    const [{ data, fetching }] = useQuery({ query: GET_CHARACTER_LIST })

    const characterGrid = (
        <Grid container spacing={2}>
            {data?.allCharacters.map(c => (
                <Grid key={c.id} size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
                    <LinkCard href={`/hermes/characters/${c.id}`}>
                        <CardContent>
                            <Typography variant="h4" styling={{ mb: "small" }}>
                                {c.name}
                            </Typography>
                        </CardContent>
                    </LinkCard>
                </Grid>
            ))}
            <Grid size={{ xs: 12, sm: 12, md: 6, lg: 4, xl: 2 }}>
                <LinkCard href="/hermes/characters/create">
                    <CardContent>
                        <Typography variant="h4" styling={{ mb: "small" }}>
                            Create New
                        </Typography>
                    </CardContent>
                </LinkCard>
            </Grid>
        </Grid>
    );

    return (
        <Box styling={{ m: "small" }}>
            {fetching ? "Loading..." : characterGrid}
        </Box>
    );
}