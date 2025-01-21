import { GenerateCompletionInput, Role } from '@/gql/graphql';
import {
    Box,
    Button,
    List,
    TextField
} from '@mui/material';
import { useEffect, useState } from 'react';
import { useMutation } from 'urql';
import { generateCompletion } from '../../mutations';
import ConversationCharacter from '../models/conversation_character';
import Message from '../models/message';
import MessageDisplay from './message_display';

interface ChatInterfaceProps {
    context: string;
    userCharacter: ConversationCharacter;
    assistantCharacter: ConversationCharacter;
}

export default function ChatInterface(props: ChatInterfaceProps) {
    const [messages, setMessages] = useState<Message[]>([]);
    const [inputText, setInputText] = useState('');
    const [sending, setSending] = useState(false);

    const [result, sendMessage] = useMutation(generateCompletion);

    useEffect(() => {
        if (!result.data?.generateCompletion.completionResponse?.chunks) {
            return;
        }

        const message = result.data?.generateCompletion.completionResponse?.chunks.map(c => c.content).join('');
        const updatedMessages = [...messages];
        updatedMessages[updatedMessages.length - 1].content = message;
        setMessages(updatedMessages);
    }, [result.data?.generateCompletion.completionResponse?.chunks])

    const handleSendMessage = async () => {
        if (!inputText.trim()) return;
        const userMessage: Message = { role: Role.User, content: inputText };
        const updatedMessages = [...messages, userMessage];
        setMessages(updatedMessages);
        setInputText('');
        setSending(true);

        // Send message to the assistant
        const variables: GenerateCompletionInput = {
            conversation: {
                context: props.context,
                user: {
                    name: props.userCharacter.name,
                    age: props.userCharacter.age,
                    details: props.userCharacter.details.map(d => {
                        return {
                            key: d.key,
                            value: d.value
                        }
                    }),
                },
                assistant: {
                    name: props.assistantCharacter.name,
                    age: props.assistantCharacter.age,
                    details: props.assistantCharacter.details.map(d => {
                        return {
                            key: d.key,
                            value: d.value
                        }
                    })
                },
                messages: updatedMessages.map(m => {
                    return {
                        role: m.role,
                        content: m.content
                    }
                })
            },
        };

        try {
            const assistantMessage: Message = {
                role: Role.Assistant,
                content: "..."
            };
            setMessages((prev) => [...prev, assistantMessage]);
            setSending(true);
            await sendMessage({ input: variables });
        } catch (error) {
            console.error('Error sending message:', error);
        } finally {
            setSending(false);
        }
    };

    return (
        <Box sx={{
            height: '100%',
            display: 'flex',
            flexDirection: 'column'
        }}>
            <List sx={{
                overflow: 'auto'
            }}>
                {messages.map((msg, index) => (
                    <MessageDisplay
                        key={index}
                        message={msg}
                        userCharacter={props.userCharacter}
                        assistantCharacter={props.assistantCharacter}
                    />
                ))}
            </List>

            <Box
                component="form"
                onSubmit={(e) => {
                    e.preventDefault();
                    handleSendMessage();
                }}
                sx={{
                    display: 'flex',
                    p: 2,
                    gap: 1,
                    borderTop: 1,
                    borderColor: 'divider'
                }}
            >
                <TextField
                    value={inputText}
                    onChange={(e) => setInputText(e.target.value)}
                    fullWidth
                    placeholder="Type your message..."
                    disabled={sending}
                />
                <Button
                    type="submit"
                    variant="contained"
                    color="primary"
                    disabled={sending || !inputText.trim()}
                >
                    Send
                </Button>
            </Box>
        </Box>
    );
};
