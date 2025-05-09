"use client";

import { Typography } from '@/app/components/typography';
import { CharacterForm, CharacterInput } from '@/app/hermes/components/character_form';
import { CREATE_CHARACTER } from '@/app/hermes/mutations';
import { CreateCharacterInput } from '@/gql/graphql';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { useMutation } from 'urql';

export default function CreateCharacterPage() {
    const router = useRouter();
    const [loading, setLoading] = useState(false);

    const [, createCharacter] = useMutation(CREATE_CHARACTER);

    const handleSave = async (input: CharacterInput) => {
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
        <div className="m-4">
            <Typography variant="h2" className="border-b-0">
                Create Character
            </Typography>

            <CharacterForm
                onSave={handleSave}
                loading={loading}
            />
        </div>
    );
}
