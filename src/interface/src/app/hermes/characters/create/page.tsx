// CreateCharacterPage.tsx
"use client";

import { CreateCharacterInput } from '@/gql/graphql';
import { Box, Button, Typography } from '@mui/material';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { useMutation } from 'urql';
import CharacterForm, { CharacterInput } from '../../components/character_form';
import { CREATE_CHARACTER } from '../../mutations';

export default function CreateCharacterPage() {
    const router = useRouter();
    const [loading, setLoading] = useState(false);

    const [, createCharacter] = useMutation(CREATE_CHARACTER);

    const handleSubmit = async (input: CharacterInput) => {
        setLoading(true);

        try {
            const createCharacterInput: CreateCharacterInput = {
                name: input.name,
                age: input.age,
                details: input.details.map(d => {
                    return {
                        key: d.key,
                        value: d.value
                    }
                }),
            };

            await createCharacter({ input: createCharacterInput });
            setLoading(false);
            router.push("/hermes/characters");
        } catch (err: any) {
            setLoading(false);
        }
    };

    return (
        <Box sx={{ m: "1rem" }}>
            <Box sx={{ width: "100%", mb: "1rem" }}>
                <Button LinkComponent={Link} href="/hermes/characters" variant="outlined">
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
        </Box>
    );
}
