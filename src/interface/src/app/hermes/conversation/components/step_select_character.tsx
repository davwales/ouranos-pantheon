import { CharacterInput, Role } from '@/gql/graphql';
import {
    Box,
    Button,
    Card,
    CardContent,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Grid2,
    TextField,
    Typography,
} from '@mui/material';
import { useState } from 'react';
import { useQuery } from 'urql';
import { getDetailedCharacterListQuery } from '../../queries';

interface StepSelectCharacterProps {
    role: Role;
    character: CharacterInput | null;
    setCharacter: (character: CharacterInput) => void;
}

export default function StepSelectCharacter(props: StepSelectCharacterProps) {
    const [isModifyDialogOpen, setIsModifyDialogOpen] = useState(false);
    const [modifiedCharacter, setModifiedCharacter] = useState<CharacterInput | null>(null);

    // Fetch characters
    const [{ data, fetching, error }] = useQuery({
        query: getDetailedCharacterListQuery,
    });

    const handleCharacterSelect = (char: CharacterInput) => {
        props.setCharacter(char);
    };

    const handleModifyCharacter = () => {
        if (props.character) {
            setModifiedCharacter(props.character);
        }

        setIsModifyDialogOpen(true);
    };

    const handleSaveModifiedCharacter = () => {
        props.setCharacter(modifiedCharacter!);
        setIsModifyDialogOpen(false);
    };

    return (
        <Box sx={{ mt: 2 }}>
            <Typography variant="h6">
                Select {props.role === Role.User ? 'Your' : "Assistant's"} Character
            </Typography>
            {fetching ? (
                <Typography>Loading characters...</Typography>
            ) : error ? (
                <Typography color="error">Error loading characters</Typography>
            ) : (
                <Grid2 container spacing={2}>
                    {data?.allCharacters.map(c => (
                        <Grid2 key={c.id} size={{ sm: 12, md: 6, lg: 4, xl: 2 }}>
                            <Card
                                onClick={() => handleCharacterSelect(c)}
                                sx={{
                                    cursor: 'pointer',
                                    border: props.character?.id === c.id ? '2px solid' : '2px solid transparent'
                                }}>
                                <CardContent>
                                    <Typography variant="h4" sx={{ mb: "0.5rem" }}>
                                        {c.name}
                                    </Typography>
                                    <Typography variant="body2">
                                        Age: {c.age}
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Grid2>
                    ))}
                </Grid2>
            )}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                {props.character && (
                    <Button
                        variant="outlined"
                        sx={{ mr: 1 }}
                        onClick={handleModifyCharacter}
                    >
                        Modify Character
                    </Button>
                )}
            </Box>

            {/* Modify Character Dialog */}
            <Dialog
                open={isModifyDialogOpen}
                onClose={() => setIsModifyDialogOpen(false)}
                fullWidth
            >
                <DialogTitle>Modify Character</DialogTitle>
                <DialogContent>
                    <TextField
                        label="Name"
                        value={modifiedCharacter?.name || ''}
                        onChange={(e) =>
                            setModifiedCharacter((prev) => ({
                                ...prev!,
                                name: e.target.value,
                            }))
                        }
                        fullWidth
                        margin="normal"
                    />
                    <TextField
                        label="Age"
                        type="number"
                        value={modifiedCharacter?.age || ''}
                        onChange={(e) =>
                            setModifiedCharacter((prev) => ({
                                ...prev!,
                                age: parseInt(e.target.value, 10),
                            }))
                        }
                        fullWidth
                        margin="normal"
                    />
                    {/* You can add fields to modify details if needed */}
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setIsModifyDialogOpen(false)}>Cancel</Button>
                    <Button
                        variant="contained"
                        color="primary"
                        onClick={handleSaveModifiedCharacter}
                    >
                        Save
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};
