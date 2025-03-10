import TextField from "@/app/components/core/inputs/text_field";

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
            onChange={props.setContext}
            multiline
            rows={4}
            variant="outlined"
            fullWidth
            required
        />
    );
};
