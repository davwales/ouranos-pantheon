import { FooterContent } from "@/app/components/footer";
import ChatInput from "@/app/hermes/conversation/components/chat_input";
import ChatMessageList from "@/app/hermes/conversation/components/chat_message_list";
import { GENERATE_COMPLETION } from "@/app/hermes/mutations";
import ConversationAssistant from "@/app/hermes/types";
import { GenerateCompletionInput, MessageInput, Role } from "@/gql/graphql";
import { useEffect, useState } from "react";
import { useMutation } from "urql";

export default function ChatInterfaceView({
  assistant,
}: {
  assistant: ConversationAssistant;
}) {
  const [messages, setMessages] = useState<MessageInput[]>([]);
  const [inputText, setInputText] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [editingMessageIndex, setEditingMessageIndex] = useState<number | null>(
    null
  );

  const [result, sendMessage] = useMutation(GENERATE_COMPLETION);

  useEffect(() => {
    if (!result.data?.generateCompletion.completionResponse?.chunks) {
      return;
    }

    const message = result.data?.generateCompletion.completionResponse?.chunks
      .map((c) => c.content)
      .join("");
    const updatedMessages = [...messages];
    updatedMessages[updatedMessages.length - 1].content = message;
    setMessages(updatedMessages);
  }, [result.data?.generateCompletion.completionResponse?.chunks]);

  const generateCompletion = async (currentMessages: MessageInput[]) => {
    if (isGenerating) return;
    setIsGenerating(true);

    const variables: GenerateCompletionInput = {
      conversation: {
        assistant: {
          model: assistant.model,
          systemPrompt: assistant.systemPrompt,
          temperature: assistant.temperature,
          maxTokens: assistant.maxTokens,
          repeatPenalty: assistant.repeatPenalty,
        },
        messages: currentMessages.map(({ role, content }) => ({
          role,
          content,
        })),
      },
    };

    try {
      const assistantMessage: MessageInput = {
        role: Role.Assistant,
        content: "...",
      };
      setMessages((prev) => [...prev, assistantMessage]);
      setIsGenerating(true);
      await sendMessage({ input: variables });
    } catch (error) {
      console.error("Error sending message:", error);
    } finally {
      setIsGenerating(false);
    }
  };

  const handleUpdateMessage = () => {
    if (!editingMessageIndex || !inputText.trim()) return;
    setMessages((prev) => {
      const updatedMessages = [...prev];
      updatedMessages[editingMessageIndex].content = inputText;
      return updatedMessages;
    });
    setEditingMessageIndex(null);
    setInputText("");
  };

  const handleNewMessage = async () => {
    if (!inputText.trim()) return;
    const userMessage: MessageInput = { role: Role.User, content: inputText };
    const updatedMessages = [...messages, userMessage];
    setMessages(updatedMessages);
    setInputText("");
    await generateCompletion(updatedMessages);
  };

  const handleMessageEdit = (index: number) => {
    setInputText(messages[index].content);
    setEditingMessageIndex(index);
  };

  const handleCancelEdit = () => {
    setEditingMessageIndex(null);
    setInputText("");
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
    <div>
      <ChatMessageList
        messages={messages}
        assistant={assistant}
        onDeleteMessage={handleMessageDeleted}
        onEditMessage={handleMessageEdit}
        onRetryMessage={handleMessageRetry}
        isGenerating={isGenerating}
      />

      <FooterContent>
        <ChatInput
          inputText={inputText}
          isGenerating={isGenerating}
          isEditing={editingMessageIndex !== null}
          onInputChange={setInputText}
          onNewMessage={handleNewMessage}
          onUpdateMessage={handleUpdateMessage}
          onCancelEdit={handleCancelEdit}
          className="p-4 border-t"
        />
      </FooterContent>
    </div>
  );
}
