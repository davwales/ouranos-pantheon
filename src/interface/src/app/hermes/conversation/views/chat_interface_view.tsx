import { FooterContent } from "@/app/components/footer";
import ChatInput from "@/app/hermes/conversation/components/chat_input";
import ChatMessageList from "@/app/hermes/conversation/components/chat_message_list";
import ConversationAssistant from "@/app/hermes/types";
import { MessageInput, Role, streamCompletion } from "@/lib/api/hermes";
import { useState } from "react";

export default function ChatInterfaceView({
  assistant,
  ...props
}: React.ComponentProps<"div"> & {
  assistant: ConversationAssistant;
}) {
  const [messages, setMessages] = useState<MessageInput[]>([]);
  const [inputText, setInputText] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [editingMessageIndex, setEditingMessageIndex] = useState<number | null>(
    null
  );

  const generateCompletion = async (currentMessages: MessageInput[]) => {
    if (isGenerating) return;
    setIsGenerating(true);

    const assistantMessage: MessageInput = { role: Role.Assistant, content: "" };
    setMessages((prev) => [...prev, assistantMessage]);

    try {
      for await (const chunk of streamCompletion({
        conversation: {
          assistant: {
            model: assistant.model,
            systemPrompt: assistant.systemPrompt,
            temperature: assistant.temperature,
            maxTokens: assistant.maxTokens,
            repeatPenalty: assistant.repeatPenalty,
          },
          messages: currentMessages.map(({ role, content }) => ({ role, content })),
        },
      })) {
        setMessages((prev) => {
          const updated = [...prev];
          updated[updated.length - 1] = {
            ...updated[updated.length - 1],
            content: updated[updated.length - 1].content + chunk.content,
          };
          return updated;
        });
      }
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
    <div {...props}>
      <ChatMessageList
        messages={messages}
        assistant={assistant}
        onDeleteMessage={handleMessageDeleted}
        onEditMessage={handleMessageEdit}
        onRetryMessage={handleMessageRetry}
        isGenerating={isGenerating}
        className="mb-2"
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
