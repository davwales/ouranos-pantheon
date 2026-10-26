import ChatInput from "@/app/(hermes)/hermes/components/chat-input";
import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const IMAGE: AssistantChatImagePart = {
  kind: "image",
  id: "part-0",
  callId: "",
  imageId: "img-1",
  url: "/api/hermes/images/img-1",
  width: 64,
  height: 64,
  altText: "A red fox",
};

function renderInput(props: Partial<React.ComponentProps<typeof ChatInput>> = {}) {
  const handlers = {
    onInputChange: vi.fn(),
    onNewMessage: vi.fn(),
    onUpdateMessage: vi.fn(),
    onRemoveAttachment: vi.fn(),
  };
  render(
    <ChatInput
      inputText=""
      isGenerating={false}
      isEditing={false}
      {...handlers}
      {...props}
    />,
  );
  return handlers;
}

describe("ChatInput", () => {
  it("WhenAnImageIsAttached_ShouldShowItAndAskForTheChange", () => {
    // Arrange & Act
    renderInput({ attachment: IMAGE });

    // Assert
    expect(screen.getByText("Editing image")).toBeInTheDocument();
    expect(screen.getByText("A red fox")).toBeInTheDocument();
    const textarea = screen.getByPlaceholderText("Describe how to change this image...");
    expect(textarea).toHaveFocus();
    expect(screen.getByRole("button", { name: "Send" })).toBeDisabled();
  });

  it("WhenRemoveIsClicked_ShouldRemoveTheAttachment", () => {
    // Arrange
    const { onRemoveAttachment } = renderInput({ attachment: IMAGE });

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Remove image" }));

    // Assert
    expect(onRemoveAttachment).toHaveBeenCalled();
  });

  it("WhenEscapeIsPressedInAnEmptyInput_ShouldRemoveTheAttachment", () => {
    // Arrange
    const { onRemoveAttachment } = renderInput({ attachment: IMAGE });

    // Act
    fireEvent.keyDown(screen.getByRole("textbox"), { key: "Escape" });

    // Assert
    expect(onRemoveAttachment).toHaveBeenCalled();
  });

  it("WhenNothingIsAttached_ShouldShowNoChip", () => {
    // Arrange & Act
    renderInput();

    // Assert
    expect(screen.queryByText("Editing image")).not.toBeInTheDocument();
    expect(screen.getByPlaceholderText("Type your message...")).toBeInTheDocument();
  });

  it("WhenTextIsTypedWithAnAttachment_ShouldSendIt", () => {
    // Arrange
    const { onNewMessage } = renderInput({ attachment: IMAGE, inputText: "Make it night" });

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Send" }));

    // Assert
    expect(onNewMessage).toHaveBeenCalled();
  });
});
