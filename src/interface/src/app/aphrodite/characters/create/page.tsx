// CreateCharacterPage.tsx
"use client";

import React, { useState } from 'react';
import { useMutation } from 'urql';
import { createCharacterMutation } from '../../mutations';
import CharacterForm, { CharacterInput } from '../components/character_form';
import { Box, Button, Typography } from '@mui/material';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

export default function CreateCharacterPage() {
    const router = useRouter();
    const [loading, setLoading] = useState(false);

    const [, createCharacter] = useMutation(createCharacterMutation);

    const handleSubmit = async (input: CharacterInput) => {
        setLoading(true);

        try {
            await createCharacter({ input });
            setLoading(false);
            router.push("/aphrodite/characters");
        } catch (err: any) {
            setLoading(false);
        }
    };

    return (
        <>
            <Box sx={{ width: "100%", mb: "1rem" }}>
                <Button LinkComponent={Link} href="/aphrodite/characters" variant="outlined">
                    Back
                </Button>
            </Box>
            <Typography variant="h5" gutterBottom>
                Create Character
            </Typography>
            <CharacterForm
                onSubmit={handleSubmit}
                loading={loading}
            />
        </>
    );
}
