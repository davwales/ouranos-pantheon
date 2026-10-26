import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { ImageActionsContext } from "@/components/shared/ai-assistant/image-actions-context";
import { UserMessage } from "@/components/shared/ai-assistant/user-message";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const IMAGE: AssistantChatImagePart = {
  kind: "image",
  id: "part-0",
  callId: "",
  imageId: "img-1",
  url: "/api/hermes/images/img-1",
  width: 64,
  height: 64,
  altText: "A fox",
};

describe("UserMessage", () => {
  it("WhenItRefersToAnImage_ShouldShowItWithoutActions", () => {
    // Arrange & Act
    render(
      <ImageActionsContext.Provider value={{ onEdit: vi.fn() }}>
        <UserMessage content="Make it night" images={[IMAGE]} />
      </ImageActionsContext.Provider>,
    );

    // Assert
    expect(screen.getByText("Make it night")).toBeInTheDocument();
    expect(screen.getByRole("img", { name: "A fox" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Edit/ })).not.toBeInTheDocument();
  });

  it("WhenItHasNoImages_ShouldShowOnlyTheText", () => {
    // Arrange & Act
    render(<UserMessage content="Hello" />);

    // Assert
    expect(screen.getByText("Hello")).toBeInTheDocument();
    expect(screen.queryByRole("img")).not.toBeInTheDocument();
  });
});
