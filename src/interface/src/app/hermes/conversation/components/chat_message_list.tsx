import ResponsiveMenu from "@/app/components/responsive_menu";
import { Role } from "@/gql/graphql";
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import ReplayIcon from '@mui/icons-material/Replay';
import { Box, List, ListItem, ListItemText, SxProps } from "@mui/material";
import { useEffect, useRef, useState } from "react";
import ConversationCharacter from "../models/conversation_character";
import Message from "../models/message";

interface ChatMessageListProps {
    sx?: SxProps;
    messages: Message[];
    userCharacter: ConversationCharacter;
    assistantCharacter: ConversationCharacter;
    onEditMessage?: (index: number) => void;
    onDeleteMessage?: (index: number) => void;
    onRetryMessage?: (index: number) => void;
    isGenerating?: boolean;
};

export default function ChatMessageList(props: ChatMessageListProps) {
    const [contextMenuAnchor, setContextMenuAnchor] = useState<HTMLElement | null>(null);
    const [selectedMessageIndex, setSelectedMessageIndex] = useState<number | null>(null);
    const messageListRef = useRef<HTMLDivElement>(null);

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
        setContextMenuAnchor(null);
        setSelectedMessageIndex(null);
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

        actions.push({
            label: 'Delete',
            icon: <DeleteIcon fontSize="small" color="error" />,
            onClick: () => props.onDeleteMessage?.(index)
        });

        return actions;
    };

    return (
        <Box sx={props.sx}>
            <List sx={{ flexGrow: 1, overflowY: 'auto', }}>
                {props.messages.map((msg, index) => (
                    <ListItem
                        key={index}
                        onClick={(e) => handleOpenContextMenu(e, index)}
                    >
                        <ListItemText
                            primary={msg.content}
                            secondary={msg.role === Role.User ? props.userCharacter.name : props.assistantCharacter.name}
                            sx={{ textAlign: msg.role === Role.User ? 'right' : 'left' }}
                        />
                    </ListItem>
                ))}
                <Box sx={{ pt: (theme) => theme.spacing(10) }} ref={messageListRef} />
            </List>

            {selectedMessageIndex !== null && (
                <ResponsiveMenu
                    anchorEl={contextMenuAnchor}
                    onClose={handleCloseContextMenu}
                    actions={getMessageActions(selectedMessageIndex, props.messages[selectedMessageIndex].role)}
                />
            )}
        </Box>
    );
}