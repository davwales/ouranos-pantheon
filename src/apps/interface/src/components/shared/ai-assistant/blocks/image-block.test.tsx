import { ImageBlock } from "@/components/shared/ai-assistant/blocks/image-block";
import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { ImageActionsContext } from "@/components/shared/ai-assistant/image-actions-context";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const IMAGE: AssistantChatImagePart = {
  kind: "image",
  id: "p1",
  callId: "c1",
  imageId: "img-1",
  url: "/api/hermes/images/img-1",
  width: 64,
  height: 32,
  altText: "A red fox in the snow",
};

describe("ImageBlock", () => {
  it("WhenRendered_ShouldLoadTheImageFromTheGateway", () => {
    // Arrange & Act
    render(<ImageBlock image={IMAGE} />);

    // Assert
    const image = screen.getByRole("img", { name: IMAGE.altText });
    expect(image.getAttribute("src")).toMatch(/\/api\/hermes\/images\/img-1$/);
    expect(image).toHaveAttribute("loading", "lazy");
  });

  it("WhenClicked_ShouldOpenTheFullImage", () => {
    // Arrange
    render(<ImageBlock image={IMAGE} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: `View image: ${IMAGE.altText}` }));

    // Assert
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Open in new tab/ })).toHaveAttribute("target", "_blank");
  });

  it("WhenTheImageFailsToLoad_ShouldSayItIsNoLongerAvailable", () => {
    // Arrange
    render(<ImageBlock image={IMAGE} />);

    // Act
    fireEvent.error(screen.getByRole("img", { name: IMAGE.altText }));

    // Assert
    expect(screen.getByText("This image is no longer available.")).toBeInTheDocument();
    expect(screen.queryByRole("img")).not.toBeInTheDocument();
  });

  it("WhenTheChatOffersNoActions_ShouldNotShowEditButtons", () => {
    // Arrange & Act
    render(<ImageBlock image={IMAGE} />);

    // Assert
    expect(screen.queryByRole("button", { name: /Edit/ })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /More like this/ })).not.toBeInTheDocument();
  });

  it("WhenTheChatOffersActions_ShouldCallThemWithTheImage", () => {
    // Arrange
    const onEdit = vi.fn();
    const onMoreLikeThis = vi.fn();
    render(
      <ImageActionsContext.Provider value={{ onEdit, onMoreLikeThis }}>
        <ImageBlock image={IMAGE} />
      </ImageActionsContext.Provider>,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: /Edit/ }));
    fireEvent.click(screen.getByRole("button", { name: /More like this/ }));

    // Assert
    expect(onEdit).toHaveBeenCalledWith(IMAGE);
    expect(onMoreLikeThis).toHaveBeenCalledWith(IMAGE);
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("WhenCompact_ShouldHideTheActions", () => {
    // Arrange & Act
    render(
      <ImageActionsContext.Provider value={{ onEdit: vi.fn() }}>
        <ImageBlock image={IMAGE} isCompact />
      </ImageActionsContext.Provider>,
    );

    // Assert
    expect(screen.queryByRole("button", { name: /Edit/ })).not.toBeInTheDocument();
  });
});
