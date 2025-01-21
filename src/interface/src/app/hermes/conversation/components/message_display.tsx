import { Role } from "@/gql/graphql";
import { ListItem, ListItemText } from "@mui/material";
import ConversationCharacter from "../models/conversation_character";
import Message from "../models/message";

interface MessageProps {
    message: Message;
    userCharacter: ConversationCharacter;
    assistantCharacter: ConversationCharacter;
};

export default function MessageDisplay(props: MessageProps) {
    return (
        <ListItem>
            <ListItemText
                primary={props.message.content}
                secondary={props.message.role === Role.User ? props.userCharacter.name : props.assistantCharacter.name}
                sx={{ textAlign: props.message.role === Role.User ? 'right' : 'left' }}
            />
        </ListItem>
    );
}