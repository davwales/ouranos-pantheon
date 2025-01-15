import { Role } from '@/gql/graphql';
import {
    Box,
    Button,
    Typography
} from '@mui/material';
import { useState } from 'react';
import CharacterForm from '../../components/character_form';
import ConversationCharacter from '../models/conversation_character';
import CharacterList from './character_list';

interface StepSelectCharacterProps {
    role: Role;
    character?: ConversationCharacter;
    setCharacter: (character: ConversationCharacter) => void;
}

export default function StepSelectCharacter(props: StepSelectCharacterProps) {
    const [isModifying, setIsModifying] = useState(false);

    const handleCharacterModified = (character: ConversationCharacter) => {
        setIsModifying(false);
        props.setCharacter(character);
    };

    return (
        <Box sx={{ mt: 2 }}>
            <Typography variant="h6" sx={{ mb: "1rem" }}>
                Select {props.role === Role.User ? 'Your' : "Assistant's"} Character
            </Typography>
            <CharacterList
                role={props.role}
                character={props.character}
                setCharacter={props.setCharacter}
                canSelect={!isModifying}
            />
            {props.character && !isModifying && (
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                    {props.character && (
                        <Button
                            variant="outlined"
                            sx={{ mr: 1 }}
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
