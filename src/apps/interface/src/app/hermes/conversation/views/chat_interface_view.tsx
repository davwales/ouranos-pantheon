"use client";

import AutosizeTextarea from "@/app/components/autosize-textarea";
import { FooterContent } from "@/app/components/footer";
import InfoCard from "@/app/components/info-card";
import { useNavBarActions } from "@/app/components/nav-bar-actions-context";
import ChatInput from "@/app/hermes/conversation/components/chat_input";
import ChatMessageList from "@/app/hermes/conversation/components/chat_message_list";
import {
  ModelFormInput,
  PersonaFormInput,
  TraitFormInput,
} from "@/app/hermes/types";
import { Button } from "@/components/ui/button";
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
import { ChevronDown, Plus, SlidersHorizontal } from "lucide-react";
import { useEffect, useState } from "react";

export default function ChatInterfaceView({
  persona,
  model,
  activeTraits,
  onPersonaChange,
  onModelChange,
  onTraitsChange,
  ...props
}: React.ComponentProps<"div"> & {
  persona: PersonaFormInput;
  model: ModelFormInput;
  activeTraits: TraitFormInput[];
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
  onTraitsChange?: (traits: TraitFormInput[]) => void;
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
          traits: activeTraits.map((t) => ({
            name: t.name,
            content: t.content,
          })),
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
        activeTraits={activeTraits}
        onPersonaChange={onPersonaChange}
        onModelChange={onModelChange}
        onTraitsChange={onTraitsChange}
      />
    </div>
  );
}

function ConversationConfigSheet({
  open,
  onOpenChange,
  persona,
  model,
  activeTraits,
  onPersonaChange,
  onModelChange,
  onTraitsChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  persona: PersonaFormInput;
  model: ModelFormInput;
  activeTraits: TraitFormInput[];
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
  onTraitsChange?: (traits: TraitFormInput[]) => void;
}) {
  const [personasState] = useApi(() => hermesApi.getAllPersonas());
  const [modelsState] = useApi(() => hermesApi.getAllModels());
  const [traitsState] = useApi(() => hermesApi.getAllTraits());
  const [personasOpen, setPersonasOpen] = useState(false);
  const [modelsOpen, setModelsOpen] = useState(false);
  const [traitsOpen, setTraitsOpen] = useState(false);
  const [ephemeralTraits, setEphemeralTraits] = useState<TraitFormInput[]>([]);
  const [isAddingTrait, setIsAddingTrait] = useState(false);
  const [draftName, setDraftName] = useState("");
  const [draftContent, setDraftContent] = useState("");

  const handleToggleTrait = (trait: TraitFormInput) => {
    if (!onTraitsChange) return;
    const isActive = activeTraits.some((t) => t.id === trait.id);
    if (isActive) {
      onTraitsChange(activeTraits.filter((t) => t.id !== trait.id));
    } else {
      onTraitsChange([...activeTraits, trait]);
    }
  };

  const handleConfirmEphemeralTrait = () => {
    if (!draftContent.trim() || !onTraitsChange) return;
    const newTrait: TraitFormInput = {
      id: crypto.randomUUID(),
      name: draftName.trim() || "Ephemeral Trait",
      content: draftContent.trim(),
    };
    setEphemeralTraits((prev) => [...prev, newTrait]);
    onTraitsChange([...activeTraits, newTrait]);
    setDraftName("");
    setDraftContent("");
    setIsAddingTrait(false);
  };

  const handleCancelEphemeralTrait = () => {
    setDraftName("");
    setDraftContent("");
    setIsAddingTrait(false);
  };

  const allTraits = [
    ...(traitsState.data?.map((t) => ({ ...t, ephemeral: false })) ?? []),
    ...ephemeralTraits.map((t) => ({ ...t, ephemeral: true })),
  ];

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

          <Collapsible open={traitsOpen} onOpenChange={setTraitsOpen}>
            <CollapsibleTrigger className="flex items-center justify-between w-full rounded-md px-2 py-2 hover:bg-accent transition-colors">
              <p className="text-sm font-medium">Traits</p>
              <ChevronDown
                className={`h-4 w-4 text-muted-foreground transition-transform ${traitsOpen ? "rotate-180" : ""}`}
              />
            </CollapsibleTrigger>
            <CollapsibleContent className="grid grid-cols-1 gap-3 mt-2">
              {allTraits.map((trait) => {
                const isActive = activeTraits.some(
                  (t) => t === trait || (t.id && t.id === trait.id),
                );
                return (
                  <InfoCard
                    key={trait.id}
                    label={trait.name}
                    description={trait.content}
                    onClick={() => handleToggleTrait(trait)}
                    className={[
                      "hover:bg-accent hover:cursor-pointer w-full",
                      trait.ephemeral ? "border-dashed" : "",
                      isActive ? "border-accent-foreground" : "",
                    ].join(" ")}
                  />
                );
              })}

              {isAddingTrait ? (
                <div className="rounded-4xl border-2 border-dashed border-accent py-4 px-3 space-y-3">
                  <input
                    type="text"
                    value={draftName}
                    onChange={(e) => setDraftName(e.target.value)}
                    placeholder="Name (optional)"
                    className="w-full text-sm font-medium bg-transparent outline-none placeholder:text-muted-foreground"
                    autoFocus
                  />
                  <AutosizeTextarea
                    value={draftContent}
                    onChange={(e) => setDraftContent(e.target.value)}
                    placeholder="Enter context or instruction to inject..."
                    className="w-full text-sm bg-transparent border-none shadow-none resize-none p-0 focus-visible:ring-0 placeholder:text-muted-foreground"
                  />
                  <div className="flex gap-2 pt-1">
                    <Button
                      size="sm"
                      onClick={handleConfirmEphemeralTrait}
                      disabled={!draftContent.trim()}
                      className="flex-1"
                    >
                      Add
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={handleCancelEphemeralTrait}
                      className="flex-1"
                    >
                      Cancel
                    </Button>
                  </div>
                </div>
              ) : (
                <button
                  onClick={() => setIsAddingTrait(true)}
                  className="flex items-center justify-center gap-2 w-full rounded-4xl border-2 border-dashed border-accent py-4 px-3 text-sm text-muted-foreground hover:text-foreground hover:border-accent-foreground transition-colors"
                >
                  <Plus className="h-4 w-4" />
                  Add ephemeral trait
                </button>
              )}
            </CollapsibleContent>
          </Collapsible>
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
