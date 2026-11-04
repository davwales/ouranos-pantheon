import { act, renderHook } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { type SavedConversation } from "@/lib/api/hermes";

import { useSavedConversationSettings } from "./use-saved-conversation-settings";

const saved = {
  id: "conversation-1",
  name: "Trip planning",
  isPublic: true,
  folderId: "folder-1",
  reasoningEffort: "high",
  toolSetId: "tool-set-1",
  persona: {
    id: "persona-1",
    name: "Guide",
    description: "",
    personality: "",
    scenario: "",
  },
  model: {
    id: "model-1",
    name: "Model",
    modelIdentifier: "model",
    systemPrompt: "",
    temperature: 0.7,
    maxTokens: 1024,
    defaultToolSetId: null,
    contextWindow: 8192,
  },
  traits: [],
  loras: [{ id: "lora-1", name: "Lora", strength: 1 }],
  messages: [],
  createdAt: "2026-10-31T12:00:00Z",
  updatedAt: "2026-10-31T12:00:00Z",
} as unknown as SavedConversation;

describe("useSavedConversationSettings", () => {
  it("starts from the saved conversation's settings", () => {
    // Arrange & Act
    const { result } = renderHook(() => useSavedConversationSettings(saved));

    // Assert
    expect(result.current.persona?.id).toBe("persona-1");
    expect(result.current.model?.id).toBe("model-1");
    expect(result.current.loraIds).toEqual(["lora-1"]);
    expect(result.current.isPublic).toBe(true);
    expect(result.current.reasoningEffort).toBe("high");
    expect(result.current.toolSetId).toBe("tool-set-1");
    expect(result.current.folderId).toBe("folder-1");
  });

  it("follows local changes over the saved values, including cleared ones", () => {
    // Arrange
    const { result } = renderHook(() => useSavedConversationSettings(saved));

    // Act
    act(() => {
      result.current.setIsPublic(false);
      result.current.setReasoningEffort(null);
      result.current.setToolSetId(null);
      result.current.setFolderId(null);
      result.current.setActiveLoras([]);
    });

    // Assert
    expect(result.current.isPublic).toBe(false);
    expect(result.current.reasoningEffort).toBeNull();
    expect(result.current.toolSetId).toBeNull();
    expect(result.current.folderId).toBeNull();
    expect(result.current.loraIds).toEqual([]);
  });

  it("has no persona or model before the conversation loads", () => {
    // Arrange & Act
    const { result } = renderHook(() => useSavedConversationSettings(undefined));

    // Assert
    expect(result.current.persona).toBeUndefined();
    expect(result.current.model).toBeUndefined();
    expect(result.current.traits).toEqual([]);
    expect(result.current.folderId).toBeNull();
  });
});
