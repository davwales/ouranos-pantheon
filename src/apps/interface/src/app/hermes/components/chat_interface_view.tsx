"use client";

import AutosizeTextarea from "@/app/components/autosize-textarea";
import { FooterContent } from "@/app/components/footer";
import InfoCard from "@/app/components/info-card";
import { useNavBarActions } from "@/app/components/nav-bar-actions-context";
import ChatInput from "@/app/hermes/components/chat_input";
import ChatMessageList from "@/app/hermes/components/chat_message_list";
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
import {
  Bookmark,
  ChevronDown,
  Plus,
  SlidersHorizontal,
  Trash2,
} from "lucide-react";
import { useCallback, useEffect, useState } from "react";

export default function ChatInterfaceView({
  persona,
  model,
  activeTraits,
  onPersonaChange,
  onModelChange,
  onTraitsChange,
  conversationId,
  conversationName,
  conversationIsPublic = true,
  initialMessages,
  onConversationSaved,
  onRename,
  onDelete,
  onVisibilityChange,
  ...props
}: React.ComponentProps<"div"> & {
  persona: PersonaFormInput;
  model: ModelFormInput;
  activeTraits: TraitFormInput[];
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
  onTraitsChange?: (traits: TraitFormInput[]) => void;
  conversationId?: string;
  conversationName?: string;
  conversationIsPublic?: boolean;
  initialMessages?: MessageInput[];
  onConversationSaved?: (id: string, name: string) => void;
  onRename?: (name: string) => void;
  onDelete?: () => void;
  onVisibilityChange?: (isPublic: boolean) => void;
}) {
  const [messages, setMessages] = useState<MessageInput[]>(
    initialMessages ?? [],
  );
  const [inputText, setInputText] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [editingMessageIndex, setEditingMessageIndex] = useState<number | null>(
    null,
  );
  const [isConfigOpen, setIsConfigOpen] = useState(false);
  const { setActions, clearActions } = useNavBarActions();

  const buildUpdatePayload = useCallback(
    (overrides: {
      messages?: MessageInput[];
      personaId?: string;
      modelConfigId?: string;
      traitIds?: string[];
    }) => ({
      name: conversationName ?? "New Conversation",
      personaId: overrides.personaId ?? persona.id!,
      modelConfigId: overrides.modelConfigId ?? model.id!,
      traitIds:
        overrides.traitIds ??
        activeTraits.filter((t) => t.id && !t.isEphemeral).map((t) => t.id!),
      messages: overrides.messages ?? messages,
      isPublic: conversationIsPublic,
    }),
    [
      conversationName,
      conversationIsPublic,
      persona.id,
      model.id,
      activeTraits,
      messages,
    ],
  );

  const handleSaveConversation = useCallback(async () => {
    try {
      const result = await hermesApi.createConversation({
        personaId: persona.id!,
        modelConfigId: model.id!,
        traitIds: activeTraits
          .filter((t) => t.id && !t.isEphemeral)
          .map((t) => t.id!),
        messages: messages,
      });
      onConversationSaved?.(result.id, result.name);
    } catch (error) {
      console.error("Error saving conversation:", error);
    }
  }, [persona.id, model.id, activeTraits, messages, onConversationSaved]);

  useEffect(() => {
    const actions = (
      <div className="flex items-center gap-1">
        {!conversationId && messages.length > 0 && !isGenerating && (
          <button
            onClick={handleSaveConversation}
            className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors"
            aria-label="Save conversation"
          >
            <Bookmark className="h-5 w-5" />
          </button>
        )}
        <button
          onClick={() => setIsConfigOpen(true)}
          disabled={isGenerating}
          className="p-1.5 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          aria-label="Configure conversation"
        >
          <SlidersHorizontal className="h-5 w-5" />
        </button>
      </div>
    );
    setActions(actions);
    return () => clearActions();
  }, [
    setActions,
    clearActions,
    handleSaveConversation,
    messages.length,
    conversationId,
    isGenerating,
  ]);

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
        ...(conversationId ? { conversationId } : {}),
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
      if (conversationId) {
        hermesApi
          .updateConversation(
            conversationId,
            buildUpdatePayload({ messages: updatedMessages }),
          )
          .catch((err) => console.error("Failed to update conversation:", err));
      }
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
    setMessages((prev) => {
      const updatedMessages = prev.filter((_, i) => i < index);
      if (conversationId) {
        hermesApi
          .updateConversation(
            conversationId,
            buildUpdatePayload({ messages: updatedMessages }),
          )
          .catch((err) => console.error("Failed to update conversation:", err));
      }
      return updatedMessages;
    });
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
        conversationName={conversationName}
        conversationIsPublic={conversationIsPublic}
        onRename={onRename}
        onDelete={onDelete}
        onVisibilityChange={onVisibilityChange}
        onPersonaChange={(p) => {
          onPersonaChange?.(p);
          if (conversationId && p.id) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({ personaId: p.id }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
        onModelChange={(m) => {
          onModelChange?.(m);
          if (conversationId && m.id) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({ modelConfigId: m.id }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
        onTraitsChange={(traits) => {
          onTraitsChange?.(traits);
          if (conversationId) {
            hermesApi
              .updateConversation(
                conversationId,
                buildUpdatePayload({
                  traitIds: traits
                    .filter((t) => t.id && !t.isEphemeral)
                    .map((t) => t.id!),
                }),
              )
              .catch((err) =>
                console.error("Failed to update conversation:", err),
              );
          }
        }}
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
  conversationName,
  conversationIsPublic,
  onRename,
  onDelete,
  onVisibilityChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  persona: PersonaFormInput;
  model: ModelFormInput;
  activeTraits: TraitFormInput[];
  onPersonaChange?: (persona: PersonaFormInput) => void;
  onModelChange?: (model: ModelFormInput) => void;
  onTraitsChange?: (traits: TraitFormInput[]) => void;
  conversationName?: string;
  conversationIsPublic?: boolean;
  onRename?: (name: string) => void;
  onDelete?: () => void;
  onVisibilityChange?: (isPublic: boolean) => void;
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
  const [nameInput, setNameInput] = useState(conversationName ?? "");

  useEffect(() => {
    setNameInput(conversationName ?? "");
  }, [conversationName]);

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
      isPublic: true,
      isEphemeral: true,
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

          {(onRename ?? onDelete ?? onVisibilityChange) && (
            <>
              <hr className="border-border" />
              <div className="space-y-3">
                <p className="text-sm font-medium px-2">Conversation</p>
                {onVisibilityChange && (
                  <label className="flex items-center justify-between px-2 py-1 cursor-pointer">
                    <span className="text-sm">Public</span>
                    <input
                      type="checkbox"
                      checked={conversationIsPublic ?? false}
                      onChange={(e) => onVisibilityChange(e.target.checked)}
                      className="h-4 w-4 cursor-pointer"
                    />
                  </label>
                )}
                {onRename && (
                  <div className="flex gap-2">
                    <input
                      value={nameInput}
                      onChange={(e) => setNameInput(e.target.value)}
                      onKeyDown={(e) => {
                        if (
                          e.key === "Enter" &&
                          nameInput.trim() &&
                          nameInput.trim() !== conversationName
                        ) {
                          onRename(nameInput.trim());
                        }
                      }}
                      className="flex-1 text-sm bg-transparent border rounded-md px-3 py-2 outline-none focus:ring-1 focus:ring-ring"
                      placeholder="Conversation name"
                    />
                    <Button
                      size="sm"
                      variant="outline"
                      disabled={
                        !nameInput.trim() ||
                        nameInput.trim() === conversationName
                      }
                      onClick={() => onRename(nameInput.trim())}
                    >
                      Rename
                    </Button>
                  </div>
                )}
                {onDelete && (
                  <Button
                    variant="destructive"
                    className="w-full"
                    onClick={onDelete}
                  >
                    <Trash2 className="h-4 w-4 mr-2" />
                    Delete Conversation
                  </Button>
                )}
              </div>
            </>
          )}
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
