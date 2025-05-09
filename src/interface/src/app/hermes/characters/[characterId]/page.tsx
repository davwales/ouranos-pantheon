"use client";

import { Typography } from '@/app/components/typography';
import { CharacterForm, CharacterInput } from '@/app/hermes/components/character_form';
import { DELETE_CHARACTER, UPDATE_CHARACTER } from '@/app/hermes/mutations';
import { GET_CHARACTER } from '@/app/hermes/queries';
import { mapDetails } from '@/app/hermes/utils/map_details';
import { UpdateCharacterInput } from '@/gql/graphql';
import { useParams, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { useMutation, useQuery } from 'urql';

export default function EditCharacterPage() {
    const router = useRouter();
    const { characterId } = useParams<{ characterId: string }>();
    const [loading, setLoading] = useState(false);
    const [initialValues, setInitialValues] = useState<CharacterInput | null>(
        null
    );

    const [{ data, fetching, error: fetchError }] = useQuery({
        query: GET_CHARACTER,
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

    const [, updateCharacter] = useMutation(UPDATE_CHARACTER);
    const [, deleteCharacter] = useMutation(DELETE_CHARACTER);

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

    const handleSave = async (input: CharacterInput) => {
        setLoading(true);

        try {
            const updateCharacterInput: UpdateCharacterInput = {
                characterId,
                name: input.name,
                age: input.age,
                details: mapDetails(input.details)
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
        <div className="m-4">
            <Typography variant="h2" className="border-b-0">
                Edit Character
            </Typography>

            <CharacterForm
                initialValues={initialValues}
                onSave={handleSave}
                onDelete={handleDelete}
                loading={loading || fetching}
                className="mt-4"
            />
        </div>
    );
}
