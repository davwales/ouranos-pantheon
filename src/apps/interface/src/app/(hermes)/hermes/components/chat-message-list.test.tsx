import ChatMessageList from "@/app/(hermes)/hermes/components/chat-message-list";
import { type AssistantChatPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { type ChatMessage } from "@/hooks/use-message-controller";
import { Role } from "@/lib/api/hermes";
import { fireEvent, render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const MESSAGES: ChatMessage[] = [
  { clientId: "user-1", role: Role.User, content: "Draw a fox" },
  { clientId: "assistant-1", role: Role.Assistant, content: "Here is a fox." },
];

function toolbars() {
  return screen.getAllByRole("toolbar", { name: "Message actions" });
}

function buttonNames(toolbar: HTMLElement) {
  return within(toolbar)
    .getAllByRole("button")
    .map((button) => button.getAttribute("aria-label"));
}

describe("ChatMessageList", () => {
  it("ShouldShowEachMessagesActionsInline", () => {
    // Arrange & Act
    render(<ChatMessageList messages={MESSAGES} personaName="Hermes" />);

    // Assert
    const [user, assistant] = toolbars();
    expect(buttonNames(user)).toEqual(["Copy", "Edit", "Delete"]);
    expect(buttonNames(assistant)).toEqual(["Copy", "Edit", "Retry"]);
    expect(assistant).toHaveClass("opacity-100");
    expect(user).toHaveClass("opacity-0");
  });

  it("WhenAnInlineActionIsClicked_ShouldRunItForThatMessage", () => {
    // Arrange
    const onRetryMessage = vi.fn();
    const onDeleteMessage = vi.fn();
    render(
      <ChatMessageList
        messages={MESSAGES}
        personaName="Hermes"
        onRetryMessage={onRetryMessage}
        onDeleteMessage={onDeleteMessage}
      />,
    );
    const [user, assistant] = toolbars();

    // Act
    fireEvent.click(within(assistant).getByRole("button", { name: "Retry" }));
    fireEvent.click(within(user).getByRole("button", { name: "Delete" }));

    // Assert
    expect(onRetryMessage).toHaveBeenCalledWith(1);
    expect(onDeleteMessage).toHaveBeenCalledWith(0);
  });

  it("WhenCopyIsClicked_ShouldCopyTheMessageContent", () => {
    // Arrange
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, "clipboard", { configurable: true, value: { writeText } });
    render(<ChatMessageList messages={MESSAGES} personaName="Hermes" />);

    // Act
    fireEvent.click(within(toolbars()[1]).getByRole("button", { name: "Copy" }));

    // Assert
    expect(writeText).toHaveBeenCalledWith("Here is a fox.");
  });

  it("WhenGenerating_ShouldHideTheStreamingMessagesActionsAndDisableTheRest", () => {
    // Arrange
    const streaming: ChatMessage[] = [
      MESSAGES[0],
      { clientId: "assistant-2", role: Role.Assistant, content: "" },
    ];
    const parts: AssistantChatPart[] = [{ kind: "text", id: "text-0", content: "Draw" }];

    // Act
    render(
      <ChatMessageList
        messages={streaming}
        streamingParts={parts}
        personaName="Hermes"
        isGenerating
      />,
    );

    // Assert
    const [user] = toolbars();
    expect(toolbars()).toHaveLength(1);
    within(user)
      .getAllByRole("button")
      .forEach((button) => expect(button).toBeDisabled());
  });

  it("WhenAnEarlierMessageIsRemoved_ShouldKeepTheLaterMessagesElement", () => {
    // Arrange
    const summary: ChatMessage = { clientId: "summary-1", role: Role.Summary, content: "Earlier" };
    const { rerender } = render(
      <ChatMessageList messages={[summary, ...MESSAGES]} personaName="Hermes" />,
    );
    const before = screen.getByText("Here is a fox.").closest(".group\\/message");

    // Act
    rerender(<ChatMessageList messages={MESSAGES} personaName="Hermes" />);

    // Assert
    expect(before).not.toBeNull();
    expect(screen.getByText("Here is a fox.").closest(".group\\/message")).toBe(before);
  });

  it("WhenScrolledAwayFromTheBottom_ShouldOfferAJumpToLatest", () => {
    // Arrange
    render(
      <div data-testid="scroller" style={{ overflowY: "auto" }}>
        <ChatMessageList messages={MESSAGES} personaName="Hermes" />
      </div>,
    );
    const scroller = screen.getByTestId("scroller");
    Object.defineProperty(scroller, "clientHeight", { configurable: true, value: 500 });
    Object.defineProperty(scroller, "scrollHeight", { configurable: true, value: 2000 });
    scroller.scrollTop = 1500;
    fireEvent.scroll(scroller);

    // Act
    scroller.scrollTop = 400;
    fireEvent.scroll(scroller);
    fireEvent.click(screen.getByRole("button", { name: "Jump to latest" }));

    // Assert
    expect(scroller.scrollTop).toBe(2000);
    expect(screen.queryByRole("button", { name: "Jump to latest" })).not.toBeInTheDocument();
  });
});
