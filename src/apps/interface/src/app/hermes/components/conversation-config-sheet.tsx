"use client";

import AutosizeTextarea from "@/app/components/autosize-textarea";
import InfoCard from "@/app/components/info-card";
import { ContextUsageBar } from "@/app/hermes/components/context-usage-bar";
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
import { hermesApi } from "@/lib/api/hermes";
import { generateID } from "@/lib/utils";
import { ChevronDown, Minimize2, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { SystemPromptView } from "@/app/hermes/components/system-prompt-view";

export function ConversationConfigSheet({
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
  tokenUsage,
  contextWindow,
  isCompacting,
  onCompact,
  composedSystemPrompt,
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
  tokenUsage?: {
    inputTokens: number;
    outputTokens: number;
    totalTokens: number;
  } | null;
  contextWindow?: number | null;
  isCompacting?: boolean;
  onCompact?: () => void;
  composedSystemPrompt?: string | null;
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
  const [prevConversationName, setPrevConversationName] =
    useState(conversationName);

  if (conversationName !== prevConversationName) {
    setPrevConversationName(conversationName);
    setNameInput(conversationName ?? "");
  }

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
      id: generateID(),
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
          {contextWindow && tokenUsage && (
            <div className="space-y-2">
              <p className="text-sm font-medium px-2">Context Usage</p>
              <ContextUsageBar
                tokenUsage={tokenUsage}
                contextWindow={contextWindow}
              />
              {onCompact && (
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full"
                  onClick={onCompact}
                  disabled={isCompacting}
                >
                  <Minimize2 className="h-4 w-4 mr-2" />
                  {isCompacting ? "Compacting..." : "Compact Conversation"}
                </Button>
              )}
            </div>
          )}
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

          {composedSystemPrompt && (
            <Collapsible defaultOpen={false}>
              <CollapsibleTrigger className="flex items-center justify-between w-full rounded-md px-2 py-2 hover:bg-accent transition-colors">
                <p className="text-sm font-medium">System Prompt</p>
                <ChevronDown className="h-4 w-4 text-muted-foreground transition-transform group-data-[state=open]:rotate-180" />
              </CollapsibleTrigger>
              <CollapsibleContent className="mt-2">
                <SystemPromptView content={composedSystemPrompt} />
              </CollapsibleContent>
            </Collapsible>
          )}

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
