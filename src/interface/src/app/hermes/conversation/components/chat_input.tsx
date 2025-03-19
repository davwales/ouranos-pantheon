import Button from '@/app/components/core/inputs/button';
import TextField from '@/app/components/core/inputs/text_field';
import FormBox from '@/app/components/core/layout/form_box';
import { StyleProps } from '@/app/components/core/style_props';
import AppBar from '@/app/components/core/surfaces/app_bar';

interface ChatInputProps {
    styling?: StyleProps;
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
                variant="contained"
                color="primary"
                disabled={props.isGenerating || !props.inputText.trim()}
                submit
            >
                Save
            </Button>
        </>
    );

    const newMessageInput = (
        <>
            <Button
                variant="contained"
                color="primary"
                disabled={props.isGenerating || !props.inputText.trim()}
                submit
            >
                Send
            </Button>
        </>
    );

    const inputPlaceholder = props.isEditing ? "Editing message..." : "Type your message...";
    const placeholder = props.isGenerating ? "Generating response..." : inputPlaceholder;

    return (
        <AppBar position='fixed' styling={{ top: 'auto', bottom: 0 }}>
            <FormBox
                onSubmit={handleSubmit}
                styling={{
                    ...props.styling,
                    display: 'flex',
                    mx: 'auto',
                    width: '100%',
                    my: 'medium',
                    gap: 'small'
                }}
            >
                <TextField
                    value={props.inputText}
                    onChange={props.onInputChange}
                    fullWidth
                    multiline
                    maxRows={4}
                    placeholder={placeholder}
                    disabled={props.isGenerating}
                />
                {props.isEditing ? editInputs : newMessageInput}
            </FormBox>
        </AppBar>
    );
}