"use client";

import { AssistantChat } from "@/components/shared/ai-assistant/assistant-chat";
import { useAssistantChat } from "@/components/shared/ai-assistant/use-assistant-chat";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { useIsMobile } from "@/hooks/use-mobile";
import { type AssistantEndpoint } from "@/lib/api/assistant";
import { RotateCcw, X } from "lucide-react";

export type AssistantPanelProps<TContext> = {
  title: string;
  description?: string;
  endpoint: AssistantEndpoint<TContext>;
  context: TContext;
  assistantName?: string;
  placeholder?: string;
  emptyState?: React.ReactNode;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function AssistantPanel<TContext>({
  title,
  description = "Chat with the AI assistant.",
  endpoint,
  context,
  assistantName = "Assistant",
  placeholder,
  emptyState,
  open,
  onOpenChange,
}: AssistantPanelProps<TContext>) {
  const isMobile = useIsMobile();
  const { messages, isStreaming, error, send, stop, reset } = useAssistantChat({
    endpoint,
    context,
  });

  const newChatButton =
    messages.length > 0 ? (
      <Button variant="ghost" size="sm" className="shrink-0" onClick={reset}>
        <RotateCcw />
        New chat
      </Button>
    ) : null;

  const chat = (
    <AssistantChat
      messages={messages}
      isStreaming={isStreaming}
      error={error}
      assistantName={assistantName}
      placeholder={placeholder}
      emptyState={emptyState}
      autoFocus={!isMobile}
      onSend={send}
      onStop={stop}
    />
  );

  if (isMobile) {
    return (
      <Drawer open={open} onOpenChange={onOpenChange}>
        <DrawerContent className="h-[85dvh] data-[vaul-drawer-direction=bottom]:max-h-[85dvh]">
          <DrawerHeader className="gap-1 border-b text-left group-data-[vaul-drawer-direction=bottom]/drawer-content:text-left">
            <div className="flex h-8 items-center justify-between gap-2">
              <DrawerTitle className="truncate">{title}</DrawerTitle>
              {newChatButton}
            </div>
            <DrawerDescription>{description}</DrawerDescription>
          </DrawerHeader>
          {chat}
        </DrawerContent>
      </Drawer>
    );
  }

  // Non-modal so the page behind stays readable and scrollable while chatting about it.
  return (
    <Sheet open={open} onOpenChange={onOpenChange} modal={false}>
      <SheetContent
        side="right"
        showCloseButton={false}
        onInteractOutside={(e) => e.preventDefault()}
        className="w-full gap-0 sm:max-w-md"
      >
        <SheetHeader className="gap-1 border-b">
          <div className="flex h-8 items-center justify-between gap-2">
            <SheetTitle className="truncate">{title}</SheetTitle>
            <div className="flex shrink-0 items-center gap-1">
              {newChatButton}
              <SheetClose asChild>
                <Button variant="ghost" size="icon-sm" aria-label="Close">
                  <X />
                </Button>
              </SheetClose>
            </div>
          </div>
          <SheetDescription>{description}</SheetDescription>
        </SheetHeader>
        {chat}
      </SheetContent>
    </Sheet>
  );
}
