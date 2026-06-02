"use client";

import { useCallback } from "react";

import { hermesApi } from "@/lib/api/hermes";
import type { MessageInput } from "@/lib/api/hermes-types";

export function useMoveConversation({
  onSuccess,
}: {
  onSuccess: () => void;
}) {
  return useCallback(
    async (conversationId: string, folderId: string | null) => {
      const conversation = await hermesApi.getConversation(conversationId);
      await hermesApi.updateConversation(conversationId, {
        name: conversation.name,
        personaId: conversation.persona.id,
        modelConfigId: conversation.model.id,
        traitIds: conversation.traits.map((t) => t.id),
        messages: conversation.messages.map(
          (m): MessageInput => ({
            role: m.role,
            content: m.content,
            sortOrder: m.sortOrder,
          }),
        ),
        isPublic: conversation.isPublic,
        folderId,
      });
      onSuccess();
    },
    [onSuccess],
  );
}
