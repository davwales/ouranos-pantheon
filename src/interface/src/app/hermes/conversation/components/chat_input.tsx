import { Box, Button, SxProps, TextField } from '@mui/material';

interface ChatInputProps {
    sx?: SxProps;
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

    const inputPlaceholder = props.isEditing ? "Editing message..." : "Type your message...";
    const placeholder = props.isGenerating ? "Generating response..." : inputPlaceholder;

    return (
        <Box component="form" onSubmit={handleSubmit} sx={props.sx}>
            <TextField
                value={props.inputText}
                onChange={(e) => props.onInputChange(e.target.value)}
                fullWidth
                multiline
                maxRows={4}
                placeholder={placeholder}
                disabled={props.isGenerating}
            />
            {props.isEditing ? editInputs : newMessageInput}
        </Box>
    );
}