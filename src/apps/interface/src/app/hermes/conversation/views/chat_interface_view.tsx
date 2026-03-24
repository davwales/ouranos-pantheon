"use client";

import { FooterContent } from "@/app/components/footer";
import InfoCard from "@/app/components/info-card";
import { useNavBarActions } from "@/app/components/nav-bar-actions-context";
import ChatInput from "@/app/hermes/conversation/components/chat_input";
import ChatMessageList from "@/app/hermes/conversation/components/chat_message_list";
import { ModelFormInput, PersonaFormInput } from "@/app/hermes/types";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { useApi } from "@/hooks/use-api";
import {
  hermesApi,
  MessageInput,
  Role,
  streamCompletion,
} from "@/lib/api/hermes";
import { ChevronDown, SlidersHorizontal } from "lucide-react";
import { useEffect, useState } from "react";

export default function ChatInterfaceView({
  persona,
  model,
  onPersonaChange,
  onModelChange,
  ...props
}: React.ComponentProps<"div"> & {
  persona: PersonaFormInput;
  model: ModelFormInput;
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
}) {
  const [messages, setMessages] = useState<MessageInput[]>([]);
  const [inputText, setInputText] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [editingMessageIndex, setEditingMessageIndex] = useState<number | null>(
    null,
  );
  const [isConfigOpen, setIsConfigOpen] = useState(false);
  const { setActions, clearActions } = useNavBarActions();

  useEffect(() => {
    setActions(
      <button
        onClick={() => setIsConfigOpen(true)}
        className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors"
        aria-label="Configure conversation"
      >
        <SlidersHorizontal className="h-5 w-5" />
      </button>,
    );
    return () => clearActions();
  }, [setActions, clearActions]);

  const generateCompletion = async (currentMessages: MessageInput[]) => {
    if (isGenerating) return;
    setIsGenerating(true);

    const assistantMessage: MessageInput = {
      role: Role.Assistant,
      content: "",
    };
    setMessages((prev) => [...prev, assistantMessage]);

    try {
      for await (const chunk of streamCompletion({
        conversation: {
          model: {
            modelIdentifier: model.modelIdentifier,
            systemPrompt: model.systemPrompt,
            temperature: model.temperature,
            maxTokens: model.maxTokens,
            repeatPenalty: model.repeatPenalty,
          },
          persona: {
            name: persona.name,
            description: persona.description,
            personality: persona.personality,
            scenario: persona.scenario,
          },
          messages: currentMessages.map(({ role, content }) => ({
            role,
            content,
          })),
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
        personaName={persona.name}
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

      <ConversationConfigSheet
        open={isConfigOpen}
        onOpenChange={setIsConfigOpen}
        persona={persona}
        model={model}
        onPersonaChange={onPersonaChange}
        onModelChange={onModelChange}
      />
    </div>
  );
}

function ConversationConfigSheet({
  open,
  onOpenChange,
  persona,
  model,
  onPersonaChange,
  onModelChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  persona: PersonaFormInput;
  model: ModelFormInput;
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
}) {
  const [personasState] = useApi(() => hermesApi.getAllPersonas());
  const [modelsState] = useApi(() => hermesApi.getAllModels());
  const [personasOpen, setPersonasOpen] = useState(false);
  const [modelsOpen, setModelsOpen] = useState(false);

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="w-full sm:max-w-md overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Conversation Settings</SheetTitle>
        </SheetHeader>

        <div className="px-4 pb-4 space-y-4">
          <CollapsibleCardSection
            label="Persona"
            open={personasOpen}
            onOpenChange={setPersonasOpen}
            items={personasState.data?.map((p) => ({
              id: p.id ?? "",
              label: p.name,
              description: p.description,
              onSelect: () => onPersonaChange?.(p),
              selected: p.id === persona.id,
            }))}
          />
          <CollapsibleCardSection
            label="Model"
            open={modelsOpen}
            onOpenChange={setModelsOpen}
            items={modelsState.data?.map((m) => ({
              id: m.id ?? "",
              label: m.name,
              description: m.modelIdentifier,
              onSelect: () => onModelChange?.(m),
              selected: m.id === model.id,
            }))}
          />
        </div>
      </SheetContent>
    </Sheet>
  );
}

function CollapsibleCardSection({
  label,
  open,
  onOpenChange,
  items,
}: {
  label: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  items?: {
    id: string;
    label: string;
    description?: string | null;
    onSelect: () => void;
    selected: boolean;
  }[];
}) {
  return (
    <Collapsible open={open} onOpenChange={onOpenChange}>
      <CollapsibleTrigger className="flex items-center justify-between w-full rounded-md px-2 py-2 hover:bg-accent transition-colors">
        <p className="text-sm font-medium">{label}</p>
        <ChevronDown
          className={`h-4 w-4 text-muted-foreground transition-transform ${open ? "rotate-180" : ""}`}
        />
      </CollapsibleTrigger>
      <CollapsibleContent className="grid grid-cols-1 gap-3 mt-2">
        {items?.map((item) => (
          <InfoCard
            key={item.id}
            label={item.label}
            description={item.description}
            onClick={item.onSelect}
            className={`hover:bg-accent hover:cursor-pointer w-full ${
              item.selected ? "border-accent-foreground" : ""
            }`}
          />
        ))}
      </CollapsibleContent>
    </Collapsible>
  );
}
