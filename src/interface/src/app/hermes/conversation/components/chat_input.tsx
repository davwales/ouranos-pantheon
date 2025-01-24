import { Box, Button, TextField } from '@mui/material';

interface ChatInputProps {
    inputText: string;
    isGenerating: boolean;
    isEditing: boolean;
    onInputChange: (value: string) => void;
    onNewMessage: () => void;
    onUpdateMessage: () => void;
    onCancelEdit?: () => void;
}

export default function ChatInput(props: ChatInputProps) {
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (props.isEditing) {
            props.onUpdateMessage();
        } else {
            props.onNewMessage();
        }
    };

    const editInputs = (
        <>
            <Button
                variant="outlined"
                color="secondary"
                onClick={props.onCancelEdit}
                disabled={props.isGenerating}
            >
                Cancel
            </Button>
            <Button
                type="submit"
                variant="contained"
                color="primary"
                disabled={props.isGenerating || !props.inputText.trim()}
            >
                Save
            </Button>
        </>
    );

    const newMessageInput = (
        <>
            <Button
                type="submit"
                variant="contained"
                color="primary"
                disabled={props.isGenerating || !props.inputText.trim()}
            >
                Send
            </Button>
        </>
    );

    return (
        <Box
            component="form"
            onSubmit={handleSubmit}
            sx={{
                display: 'flex',
                p: 2,
                gap: 1,
                borderTop: 1,
                borderColor: 'divider',
                position: 'fixed',
                bottom: 0,
                left: 0,
                right: 0,
                justifyContent: 'center',
                maxWidth: 'md',
                mx: 'auto',
                backgroundColor: 'background.default',
                zIndex: 1000
            }}
        >
            <TextField
                value={props.inputText}
                onChange={(e) => props.onInputChange(e.target.value)}
                fullWidth
                placeholder={props.isEditing ? "Editing message..." : "Type your message..."}
                disabled={props.isGenerating}
            />
            {props.isEditing ? editInputs : newMessageInput}
        </Box>
    );
}