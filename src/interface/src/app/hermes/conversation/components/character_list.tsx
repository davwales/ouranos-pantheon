import { Role } from '@/gql/graphql';
import {
    Card,
    CardContent,
    Grid2,
    Typography
} from '@mui/material';
import { useQuery } from 'urql';
import { GET_DETAILED_CHARACTER_LIST } from '../../queries';
import ConversationCharacter from '../models/conversation_character';

interface StepSelectCharacterProps {
    role: Role;
    character?: ConversationCharacter;
    setCharacter: (character: ConversationCharacter) => void;
    canSelect: boolean;
}

export default function CharacterList(props: StepSelectCharacterProps) {
    const [{ data, fetching }] = useQuery({
        query: GET_DETAILED_CHARACTER_LIST,
    });

    const handleCharacterSelect = (char: ConversationCharacter) => {
        props.setCharacter(char);
    };

    const handleCharacterCreate = () => {
        props.setCharacter({
            name: '',
            age: 0,
            details: []
        });
    };

    const characterCard = (name: string, selected: boolean, onClick: () => void) => (
        <Card
            onClick={onClick}
            sx={{
                cursor: props.canSelect ? 'pointer' : 'default',
                border: selected ? '2px solid' : '2px solid transparent'
            }}
        >
            <CardContent>
                <Typography variant="h4">
                    {name}
                </Typography>
            </CardContent>
        </Card>
    );

    const isNewCharacter = (props.character && !props.character.id) ?? false;

    return (
        <>
            {fetching ? "Loading..." : (
                <Grid2 container spacing={2}>
                    {data?.allCharacters.map((c, index) => (
                        <Grid2 key={index} size={{ xs: 12, sm: 12, md: 6, lg: 4, xl: 2 }}>
                            {characterCard(c.name, props.character?.id == c.id, () => handleCharacterSelect(c))}
                        </Grid2>
                    ))}
                    <Grid2 size={{ xs: 12, sm: 12, md: 6, lg: 4, xl: 2 }}>
                        {characterCard("Create New", isNewCharacter, handleCharacterCreate)}
                    </Grid2>
                </Grid2>
            )}
        </>
    );
};
