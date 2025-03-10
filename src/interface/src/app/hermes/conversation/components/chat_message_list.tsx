import List from "@/app/components/core/data-display/list";
import ListItem from "@/app/components/core/data-display/list_item";
import ListItemText from "@/app/components/core/data-display/list_item_text";
import DeleteIcon from "@/app/components/core/icons/delete_icon";
import EditIcon from "@/app/components/core/icons/edit_icon";
import ReplayIcon from "@/app/components/core/icons/replay_icon";
import Box from "@/app/components/core/layout/box";
import { StyleProps } from "@/app/components/core/style_props";
import ResponsiveMenu from "@/app/components/navigation/responsive_menu";
import ConversationCharacter from "@/app/hermes/conversation/models/conversation_character";
import Message from "@/app/hermes/conversation/models/message";
import { Role } from "@/gql/graphql";
import { useEffect, useRef, useState } from "react";

interface ChatMessageListProps {
    styling?: StyleProps;
    messages: Message[];
    userCharacter: ConversationCharacter;
    assistantCharacter: ConversationCharacter;
    onEditMessage?: (index: number) => void;
    onDeleteMessage?: (index: number) => void;
    onRetryMessage?: (index: number) => void;
    isGenerating?: boolean;
};

export default function ChatMessageList(props: ChatMessageListProps) {
    const [contextMenuAnchor, setContextMenuAnchor] = useState<HTMLElement | undefined>();
    const [selectedMessageIndex, setSelectedMessageIndex] = useState<number | undefined>();
    const messageListRef = useRef<HTMLDivElement | null>(null);

    useEffect(() => {
        if (messageListRef?.current) {
            messageListRef.current.scrollIntoView({ behavior: 'smooth' });
        }
    }, [props.messages]);

    const handleOpenContextMenu = (event: React.MouseEvent<HTMLLIElement>, index: number) => {
        if (props.isGenerating) return;

        event.preventDefault();
        setContextMenuAnchor(event.currentTarget);
        setSelectedMessageIndex(index);
    };

    const handleCloseContextMenu = () => {
        setContextMenuAnchor(undefined);
        setSelectedMessageIndex(undefined);
    };

    const getMessageActions = (index: number, role: Role) => {
        const actions = [
            {
                label: 'Edit',
                icon: <EditIcon fontSize="small" />,
                onClick: () => props.onEditMessage?.(index)
            }
        ];

        if (role === Role.Assistant) {
            actions.push({
                label: 'Retry',
                icon: <ReplayIcon fontSize="small" />,
                onClick: () => props.onRetryMessage?.(index)
            });
        }

        if (role === Role.User) {
            actions.push({
                label: 'Delete',
                icon: <DeleteIcon fontSize="small" color="error" />,
                onClick: () => props.onDeleteMessage?.(index)
            });
        }

        return actions;
    };

    return (
        <Box styling={props.styling}>
            <List styling={{ flexGrow: 1, overflowY: 'auto', pb: 'xxl' }}>
                {props.messages.map((msg, index) => (
                    <ListItem
                        key={index}
                        onClick={(e) => handleOpenContextMenu(e, index)}
                    >
                        <ListItemText
                            primary={msg.content}
                            secondary={msg.role === Role.User ? props.userCharacter.name : props.assistantCharacter.name}
                            styling={{ textAlign: msg.role === Role.User ? 'right' : 'left' }}
                        />
                    </ListItem>
                ))}

                <div ref={messageListRef} />
            </List>

            {selectedMessageIndex && (
                <ResponsiveMenu
                    anchorEl={contextMenuAnchor}
                    onClose={handleCloseContextMenu}
                    actions={getMessageActions(selectedMessageIndex, props.messages[selectedMessageIndex].role)}
                />
            )}
        </Box>
    );
}