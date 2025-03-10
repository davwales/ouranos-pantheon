import Typography from '@/app/components/core/data-display/typography';
import Button from '@/app/components/core/inputs/button';
import Box from '@/app/components/core/layout/box';
import CharacterForm from '@/app/hermes/components/character_form';
import CharacterList from '@/app/hermes/conversation/components/character_list';
import ConversationCharacter from '@/app/hermes/conversation/models/conversation_character';
import { Role } from '@/gql/graphql';
import { useState } from 'react';

interface SelectCharacterViewProps {
    role: Role;
    character?: ConversationCharacter;
    setCharacter: (character: ConversationCharacter) => void;
}

export default function SelectCharacterView(props: SelectCharacterViewProps) {
    const [isModifying, setIsModifying] = useState(false);

    const handleCharacterModified = (character: ConversationCharacter) => {
        setIsModifying(false);
        props.setCharacter(character);
    };

    return (
        <Box styling={{ mt: "large" }}>
            <Typography variant="h6" styling={{ mb: "medium" }}>
                Select {props.role === Role.User ? 'Your' : "Assistant's"} Character
            </Typography>

            <CharacterList
                role={props.role}
                character={props.character}
                setCharacter={props.setCharacter}
                canSelect={!isModifying}
            />

            {props.character && !isModifying && (
                <Box styling={{ display: 'flex', justifyContent: 'space-between', mt: "small" }}>
                    {props.character && (
                        <Button
                            variant="outlined"
                            styling={{ mr: "small" }}
                            onClick={() => setIsModifying(true)}
                        >
                            Modify Character
                        </Button>
                    )}
                </Box>
            )}

            {isModifying && (
                <CharacterForm
                    initialValues={props.character}
                    onSubmit={handleCharacterModified}
                />
            )}
        </Box>
    );
};
