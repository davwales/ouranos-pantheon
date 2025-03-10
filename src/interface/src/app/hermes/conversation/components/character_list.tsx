import Typography from '@/app/components/core/data-display/typography';
import Grid from '@/app/components/core/layout/grid';
import Card from '@/app/components/core/surfaces/card';
import CardContent from '@/app/components/core/surfaces/card_content';
import ConversationCharacter from '@/app/hermes/conversation/models/conversation_character';
import { GET_DETAILED_CHARACTER_LIST } from '@/app/hermes/queries';
import { Role } from '@/gql/graphql';
import { useQuery } from 'urql';

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
            styling={{
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
                <Grid container spacing={2}>
                    {data?.allCharacters.map((c, index) => (
                        <Grid key={index} size={{ xs: 12, sm: 12, md: 6, lg: 4, xl: 2 }}>
                            {characterCard(c.name, props.character?.id == c.id, () => handleCharacterSelect(c))}
                        </Grid>
                    ))}
                    <Grid size={{ xs: 12, sm: 12, md: 6, lg: 4, xl: 2 }}>
                        {characterCard("Create New", isNewCharacter, handleCharacterCreate)}
                    </Grid>
                </Grid>
            )}
        </>
    );
};
