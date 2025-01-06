"use client";

import { useQuery } from "@urql/next";
import { getCharacterListQuery } from "../queries";
import LinkCard from "@/app/components/link_card";
import { Box, Button, CardContent, Grid2, Typography } from "@mui/material";
import Link from "next/link";

export default function CharactersPage() {
    const [{ data, fetching }] = useQuery({ query: getCharacterListQuery })

    const characterGrid = (
        <Grid2 container spacing={2}>
            {data?.allCharacters.map(c => (
                <Grid2 key={c.id} size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
                    <LinkCard href={`/hermes/characters/${c.id}`}>
                        <CardContent>
                            <Typography variant="h4" sx={{ mb: "0.5rem" }}>
                                {c.name}
                            </Typography>
                            <Typography variant="body2">
                                Age: {c.age}
                            </Typography>
                        </CardContent>
                    </LinkCard>
                </Grid2>
            ))}
            <Grid2 size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
                <LinkCard href="/hermes/characters/create">
                    <CardContent>
                        <Typography variant="h4" sx={{ mb: "0.5rem" }}>
                            Create New
                        </Typography>
                        <Typography variant="body2">
                            Create your own character!
                        </Typography>
                    </CardContent>
                </LinkCard>
            </Grid2>
        </Grid2>
    );

    return (
        <>
            {fetching ? "Loading..." : characterGrid}
        </>
    );
}