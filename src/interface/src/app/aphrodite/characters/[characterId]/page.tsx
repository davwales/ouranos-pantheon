// EditCharacterPage.tsx
"use client";

import React, { useState, useEffect } from 'react';
import { useMutation, useQuery } from 'urql';
import { deleteCharacterMutation, updateCharacterMutation } from '../../mutations';
import { getCharacterQuery } from '../../queries';
import { useParams, useRouter } from 'next/navigation';
import CharacterForm, { CharacterInput } from '../components/character_form';
import { Box, Button, Typography } from '@mui/material';
import Link from 'next/link';

export default function EditCharacterPage() {
    const router = useRouter();
    const { characterId } = useParams<{ characterId: string }>();
    const [loading, setLoading] = useState(false);
    const [initialValues, setInitialValues] = useState<CharacterInput | null>(
        null
    );

    // Fetch character data
    const [{ data, fetching, error: fetchError }] = useQuery({
        query: getCharacterQuery,
        variables: { characterId },
    });

    useEffect(() => {
        if (data?.character) {
            setInitialValues({
                name: data.character.name,
                age: data.character.age,
                details: data.character.details,
            });
        }
    }, [data, fetchError]);

    const [, updateCharacter] = useMutation(updateCharacterMutation);
    const [, deleteCharacter] = useMutation(deleteCharacterMutation);

    const handleDelete = () => {
        setLoading(true);

        try {
            deleteCharacter({ input: { characterId } })
            setLoading(false);
            router.push("/aphrodite/characters");
        } catch (err: any) {
            setLoading(false);
        }
    };

    const handleSubmit = async (input: CharacterInput) => {
        setLoading(true);

        try {
            await updateCharacter({ input: { characterId, ...input } });
            setLoading(false);
            router.push("/aphrodite/characters");
        } catch (err: any) {
            setLoading(false);
        }
    };

    if (fetching || !initialValues) {
        return <div>Loading...</div>;
    }

    return (
        <>
            <Box sx={{ width: "100%", mb: "1rem" }}>
                <Button LinkComponent={Link} href="/aphrodite/characters" variant="outlined">
                    Back
                </Button>
                <Button variant="outlined" color="error" onClick={handleDelete} sx={{ float: "right" }}>
                    Delete
                </Button>
            </Box>
            <Typography variant="h5" gutterBottom>
                Edit Character
            </Typography>
            <CharacterForm
                initialValues={initialValues}
                onSubmit={handleSubmit}
                loading={loading || fetching}
            />
        </>

    );
}
