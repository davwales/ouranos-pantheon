import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { ImageDialog } from "@/components/shared/ai-assistant/blocks/image-dialog";
import {
  type ImageActions,
  ImageActionsContext,
} from "@/components/shared/ai-assistant/image-actions-context";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

function image(id: string, altText: string): AssistantChatImagePart {
  return {
    kind: "image",
    id,
    callId: "",
    imageId: id,
    url: `/api/hermes/images/${id}`,
    width: 64,
    height: 64,
    altText,
  };
}

const EDITED = image("img-2", "A fox at night");
const SOURCE = image("img-1", "A fox");

function renderDialog(actions: ImageActions, onOpenChange = vi.fn()) {
  render(
    <ImageActionsContext.Provider value={actions}>
      <ImageDialog image={EDITED} open onOpenChange={onOpenChange} />
    </ImageActionsContext.Provider>,
  );
}

describe("ImageDialog", () => {
  it("WhenTheImageWasEdited_ShouldLinkToItsSourceAndBack", async () => {
    // Arrange
    const loadSource = vi.fn(async (shown: AssistantChatImagePart) =>
      shown.imageId === EDITED.imageId ? SOURCE : null,
    );
    renderDialog({ loadSource });

    // Act
    fireEvent.click(await screen.findByRole("button", { name: /Edited from/ }));

    // Assert
    expect(screen.getByText("Source image")).toBeInTheDocument();
    expect(screen.getByRole("img", { name: "A fox" })).toBeInTheDocument();

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Back" }));

    // Assert
    expect(screen.getByRole("img", { name: "A fox at night" })).toBeInTheDocument();
  });

  it("WhenTheImageHasNoSource_ShouldNotShowEditedFrom", async () => {
    // Arrange
    const loadSource = vi.fn(async () => null);
    renderDialog({ loadSource });

    // Assert
    await vi.waitFor(() => expect(loadSource).toHaveBeenCalled());
    expect(screen.queryByRole("button", { name: /Edited from/ })).not.toBeInTheDocument();
  });

  it("WhenEditIsClicked_ShouldCloseAndEditTheShownImage", () => {
    // Arrange
    const onEdit = vi.fn();
    const onOpenChange = vi.fn();
    renderDialog({ onEdit }, onOpenChange);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /Edit/ }));

    // Assert
    expect(onEdit).toHaveBeenCalledWith(EDITED);
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });
});
