"use client";

import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { createContext, useContext } from "react";

// What a chat can do with the images it shows. Every action is optional, so a chat that leaves
// one out hides its button.
export type ImageActions = {
  // The image this one was edited from, or null when it was drawn from scratch.
  loadSource?: (image: AssistantChatImagePart) => Promise<AssistantChatImagePart | null>;
  onEdit?: (image: AssistantChatImagePart) => void;
  onMoreLikeThis?: (image: AssistantChatImagePart) => void;
};

export const ImageActionsContext = createContext<ImageActions>({});

export function useImageActions(): ImageActions {
  return useContext(ImageActionsContext);
}
