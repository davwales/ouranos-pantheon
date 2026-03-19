import AutosizeTextarea from '@/app/components/autosize-textarea';
import { Button } from '@/components/ui/button';
import { ChangeEvent } from 'react';


export default function ChatInput({
    inputText,
    isGenerating,
    isEditing,
    onInputChange,
    onNewMessage,
    onUpdateMessage,
    onCancelEdit,
    className,
    ...props
}: React.ComponentProps<"form"> & {
    inputText: string;
    isGenerating: boolean;
    isEditing: boolean;
    onInputChange: (value: string) => void;
    onNewMessage: () => void;
    onUpdateMessage: () => void;
    onCancelEdit?: () => void;
}) {
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (isEditing) {
            onUpdateMessage();
        } else {
            onNewMessage();
        }
    };

    const handleInputChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
        return onInputChange(event.target.value);
    };

    const EditInputs = () => (
        <div>
            <Button
                color="primary"
                disabled={isGenerating || !inputText.trim()}
                type="submit"
                className="w-full"
            >
                Save
            </Button>
            <Button
                variant="outline"
                color="secondary"
                onClick={onCancelEdit}
                disabled={isGenerating}
                type="button"
                className="w-full mt-2"
            >
                Cancel
            </Button>
        </div>
    );

    const NewMessageInput = () => (
        <div className="h-fit">
            <Button
                color="primary"
                disabled={isGenerating || !inputText.trim()}
                type="submit"
                className="h-fit"
            >
                Send
            </Button>
        </div>
    );

    const inputPlaceholder = isEditing ? "Editing message..." : "Type your message...";
    const placeholder = isGenerating ? "Generating response..." : inputPlaceholder;

    return (
        <form {...props} onSubmit={handleSubmit} className={`flex items-center gap-4 ${className}`}>
            <AutosizeTextarea
                value={inputText}
                onChange={handleInputChange}
                placeholder={placeholder}
                disabled={isGenerating}
                className="min-h-16 max-h-64"
            />
            {isEditing ? <EditInputs /> : <NewMessageInput />}
        </form>
    );
}