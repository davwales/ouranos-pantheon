// EditCharacterPage.tsx
"use client";

import { UpdateCharacterInput } from '@/gql/graphql';
import { Box, Button, Typography } from '@mui/material';
import Link from 'next/link';
import { useParams, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { useMutation, useQuery } from 'urql';
import CharacterForm, { CharacterInput } from '../../components/character_form';
import { deleteCharacterMutation, updateCharacterMutation } from '../../mutations';
import { getCharacterQuery } from '../../queries';

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
            router.push("/hermes/characters");
        } catch (err: any) {
            setLoading(false);
        }
    };

    const handleSubmit = async (input: CharacterInput) => {
        setLoading(true);

        try {
            const updateCharacterInput: UpdateCharacterInput = {
                characterId,
                name: input.name,
                age: input.age,
                details: input.details.map(d => {
                    return {
                        key: d.key,
                        value: d.value
                    }
                }),
            };

            await updateCharacter({ input: updateCharacterInput });
            setLoading(false);
            router.push("/hermes/characters");
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
                <Button LinkComponent={Link} href="/hermes/characters" variant="outlined">
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
