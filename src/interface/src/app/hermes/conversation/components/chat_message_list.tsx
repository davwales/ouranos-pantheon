import { MenuAction, ResponsiveContextMenu } from "@/app/components/responsive-context-menu";
import { Typography } from "@/app/components/typography";
import ConversationCharacter from "@/app/hermes/conversation/models/conversation_character";
import Message from "@/app/hermes/conversation/models/message";
import { Role } from "@/gql/graphql";
import { Pencil, RotateCcw, Trash } from "lucide-react";
import { useEffect, useRef } from "react";

export default function ChatMessageList({
    messages,
    userCharacter,
    assistantCharacter,
    isGenerating,
    onEditMessage,
    onDeleteMessage,
    onRetryMessage,
    ...props
}: React.ComponentProps<"div"> & {
    messages: Message[];
    userCharacter: ConversationCharacter;
    assistantCharacter: ConversationCharacter;
    onEditMessage?: (index: number) => void;
    onDeleteMessage?: (index: number) => void;
    onRetryMessage?: (index: number) => void;
    isGenerating?: boolean;
}) {
    const messageListRef = useRef<HTMLDivElement | null>(null);

    useEffect(() => {
        if (messageListRef?.current) {
            messageListRef.current.scrollIntoView({ behavior: 'smooth' });
        }
    }, [messages]);

    const getMenuActions = (index: number): MenuAction[] => {
        const actions: MenuAction[] = [
            {
                label: 'Edit',
                icon: <Pencil />,
                onClick: () => onEditMessage?.(index)
            }
        ];

        if (messages[index].role === Role.Assistant) {
            actions.push({
                label: 'Retry',
                icon: <RotateCcw />,
                onClick: () => onRetryMessage?.(index)
            });
        }

        if (messages[index].role === Role.User) {
            actions.push({
                label: 'Delete',
                icon: <Trash />,
                onClick: () => onDeleteMessage?.(index)
            });
        }

        return actions;
    };

    return (
        <div {...props}>
            {messages.map((msg, index) => (
                <ResponsiveContextMenu
                    key={index}
                    actions={getMenuActions(index)}
                    title="Actions"
                    description="Perform an action on the message."
                    disabled={isGenerating}
                >
                    <div className={`w-fit text-left break-words mt-4 mx-2 ${msg.role == Role.User && "ml-auto"}`}>
                        <Typography className={`py-2 px-4 border rounded-2xl ${msg.role == Role.User && "bg-accent/30"}`}>
                            {msg.content}
                        </Typography>
                        <Typography variant="muted" className={`mx-2.5 my-1 ${msg.role == Role.User && "text-right"}`}>
                            {msg.role == Role.User ? userCharacter.name : assistantCharacter.name}
                        </Typography>
                    </div>
                </ResponsiveContextMenu>
            ))}
            <div ref={messageListRef} />
        </div>
    );
}