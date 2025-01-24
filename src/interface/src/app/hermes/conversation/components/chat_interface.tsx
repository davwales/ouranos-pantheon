import { GenerateCompletionInput, Role } from '@/gql/graphql';
import { Box } from '@mui/material';
import { useEffect, useState } from 'react';
import { useMutation } from 'urql';
import { GENERATE_COMPLETION } from '../../mutations';
import { mapDetails } from '../../utilities/map_details';
import ConversationCharacter from '../models/conversation_character';
import Message from '../models/message';
import ChatInput from './chat_input';
import ChatMessageList from './chat_message_list';

interface ChatInterfaceProps {
    context: string;
    userCharacter: ConversationCharacter;
    assistantCharacter: ConversationCharacter;
}

export default function ChatInterface(props: ChatInterfaceProps) {
    const [messages, setMessages] = useState<Message[]>([]);
    const [inputText, setInputText] = useState('');
    const [isGenerating, setIsGenerating] = useState(false);
    const [editingMessageIndex, setEditingMessageIndex] = useState<number | null>(null);

    const [result, sendMessage] = useMutation(GENERATE_COMPLETION);

    useEffect(() => {
        if (!result.data?.generateCompletion.completionResponse?.chunks) {
            return;
        }

        const message = result.data?.generateCompletion.completionResponse?.chunks.map(c => c.content).join('');
        const updatedMessages = [...messages];
        updatedMessages[updatedMessages.length - 1].content = message;
        setMessages(updatedMessages);
    }, [result.data?.generateCompletion.completionResponse?.chunks])

    const generateCompletion = async (currentMessages: Message[]) => {
        if (isGenerating) return;
        setIsGenerating(true);

        const variables: GenerateCompletionInput = {
            conversation: {
                context: props.context,
                user: {
                    name: props.userCharacter.name,
                    age: props.userCharacter.age,
                    details: mapDetails(props.userCharacter.details),
                },
                assistant: {
                    name: props.assistantCharacter.name,
                    age: props.assistantCharacter.age,
                    details: mapDetails(props.assistantCharacter.details),
                },
                messages: currentMessages.map(({ role, content }) => ({ role, content })),
            },
        };

        try {
            const assistantMessage: Message = {
                role: Role.Assistant,
                content: "..."
            };
            setMessages((prev) => [...prev, assistantMessage]);
            setIsGenerating(true);
            await sendMessage({ input: variables });
        } catch (error) {
            console.error('Error sending message:', error);
        } finally {
            setIsGenerating(false);
        }
    };

    const handleUpdateMessage = () => {
        if (!editingMessageIndex || !inputText.trim()) return;
        setMessages(prev => {
            const updatedMessages = [...prev];
            updatedMessages[editingMessageIndex].content = inputText;
            return updatedMessages;
        });
        setEditingMessageIndex(null);
        setInputText('');
    };

    const handleNewMessage = async () => {
        if (!inputText.trim()) return;
        const userMessage: Message = { role: Role.User, content: inputText };
        const updatedMessages = [...messages, userMessage];
        setMessages(updatedMessages);
        setInputText('');
        await generateCompletion(updatedMessages);
    };

    const handleMessageEdit = (index: number) => {
        setInputText(messages[index].content);
        setEditingMessageIndex(index);
    };

    const handleCancelEdit = () => {
        setEditingMessageIndex(null);
        setInputText('');
    };

    const handleMessageDeleted = (index: number) => {
        setMessages((prev) => prev.filter((_, i) => i < index));
    };

    const handleMessageRetry = async (index: number) => {
        const updatedMessages = messages.filter((_, i) => i < index);
        setMessages(updatedMessages);
        await generateCompletion(updatedMessages);
    };

    return (
        <Box sx={{
            height: '100%',
            display: 'flex',
            flexDirection: 'column',
            position: 'relative',
        }}>
            <ChatMessageList
                messages={messages}
                userCharacter={props.userCharacter}
                assistantCharacter={props.assistantCharacter}
                onDeleteMessage={handleMessageDeleted}
                onEditMessage={handleMessageEdit}
                onRetryMessage={handleMessageRetry}
                isGenerating={isGenerating}
            />

            <ChatInput
                inputText={inputText}
                isGenerating={isGenerating}
                isEditing={editingMessageIndex !== null}
                onInputChange={setInputText}
                onNewMessage={handleNewMessage}
                onUpdateMessage={handleUpdateMessage}
                onCancelEdit={handleCancelEdit}
            />
        </Box>
    );
};
