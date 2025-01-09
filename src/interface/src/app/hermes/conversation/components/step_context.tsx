import { TextField } from '@mui/material';

interface StepContextProps {
    context: string;
    setContext: (context: string) => void;
}

export default function StepContext(props: StepContextProps) {
    return (
        <TextField
            label="Chat Context"
            placeholder="Enter the context for the chat..."
            value={props.context}
            onChange={(e) => props.setContext(e.target.value)}
            multiline
            rows={4}
            variant="outlined"
            fullWidth
            required
        />
    );
};
