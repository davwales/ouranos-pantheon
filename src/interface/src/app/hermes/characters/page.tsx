"use client";

import LinkCard from "@/app/components/link_card";
import { Box, CardContent, Grid2, Typography } from "@mui/material";
import { useQuery } from "@urql/next";
import { GET_CHARACTER_LIST } from "../queries";

export default function CharactersPage() {
    const [{ data, fetching }] = useQuery({ query: GET_CHARACTER_LIST })

    const characterGrid = (
        <Grid2 container spacing={2}>
            {data?.allCharacters.map(c => (
                <Grid2 key={c.id} size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
                    <LinkCard href={`/hermes/characters/${c.id}`}>
                        <CardContent>
                            <Typography variant="h4" sx={{ mb: "0.5rem" }}>
                                {c.name}
                            </Typography>
                        </CardContent>
                    </LinkCard>
                </Grid2>
            ))}
            <Grid2 size={{ xs: 12, sm: 12, md: 6, lg: 4, xl: 2 }}>
                <LinkCard href="/hermes/characters/create">
                    <CardContent>
                        <Typography variant="h4" sx={{ mb: "0.5rem" }}>
                            Create New
                        </Typography>
                    </CardContent>
                </LinkCard>
            </Grid2>
        </Grid2>
    );

    return (
        <Box sx={{ m: "1rem" }}>
            {fetching ? "Loading..." : characterGrid}
        </Box>
    );
}