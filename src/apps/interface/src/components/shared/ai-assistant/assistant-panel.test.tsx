import { AssistantPanel } from "@/components/shared/ai-assistant/assistant-panel";
import { useIsMobile } from "@/hooks/use-mobile";
import {
  streamAssistant,
  type AssistantEndpoint,
  type AssistantEvent,
} from "@/lib/api/assistant";
import { fireEvent, render, screen } from "@testing-library/react";
import { useState } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/hooks/use-mobile", () => ({
  useIsMobile: vi.fn(() => false),
}));

vi.mock("@/lib/api/assistant", () => ({
  streamAssistant: vi.fn(),
}));

async function* events(...items: AssistantEvent[]): AsyncGenerator<AssistantEvent> {
  for (const item of items) {
    yield item;
  }
}

const ENDPOINT: AssistantEndpoint<{ id: string }> =
  "/api/test/assistant/completions/stream";

function TestPanel() {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button onClick={() => setOpen(true)}>Open assistant</button>
      <AssistantPanel
        title="Test Assistant"
        endpoint={ENDPOINT}
        context={{ id: "1" }}
        assistantName="Helper"
        emptyState={<p>Nothing yet</p>}
        open={open}
        onOpenChange={setOpen}
      />
    </>
  );
}

function renderPanel() {
  return render(<TestPanel />);
}

describe("AssistantPanel", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useIsMobile).mockReturnValue(false);
  });

  it("WhenTriggerClicked_ShouldOpenWithTitleAndEmptyState", async () => {
    // Arrange
    renderPanel();

    // Act
    fireEvent.click(screen.getByText("Open assistant"));

    // Assert
    expect(await screen.findByText("Test Assistant")).toBeInTheDocument();
    expect(screen.getByText("Nothing yet")).toBeInTheDocument();
  });

  it("WhenMessageSent_ShouldRenderUserMessageAndStreamedReply", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events({ $type: "content", content: "Use olive oil." }, { $type: "done" }),
    );
    renderPanel();
    fireEvent.click(screen.getByText("Open assistant"));
    const input = await screen.findByLabelText("Message");

    // Act
    fireEvent.change(input, { target: { value: "Butter substitute?" } });
    fireEvent.keyDown(input, { key: "Enter" });

    // Assert
    expect(await screen.findByText("Use olive oil.")).toBeInTheDocument();
    expect(screen.getByText("Butter substitute?")).toBeInTheDocument();
    expect(screen.getByText("Helper")).toBeInTheDocument();
  });

  it("WhenErrorEventReceived_ShouldShowAlert", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events({ $type: "error", message: "The assistant failed." }),
    );
    renderPanel();
    fireEvent.click(screen.getByText("Open assistant"));
    const input = await screen.findByLabelText("Message");

    // Act
    fireEvent.change(input, { target: { value: "Hi" } });
    fireEvent.click(screen.getByLabelText("Send"));

    // Assert
    expect(await screen.findByRole("alert")).toHaveTextContent("The assistant failed.");
  });

  it("WhenNoMessages_ShouldHideNewChat", async () => {
    // Arrange
    renderPanel();

    // Act
    fireEvent.click(screen.getByText("Open assistant"));

    // Assert
    expect(await screen.findByText("Test Assistant")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "New chat" })).not.toBeInTheDocument();
  });

  it("WhenNewChatClicked_ShouldClearConversation", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events({ $type: "content", content: "Use olive oil." }, { $type: "done" }),
    );
    renderPanel();
    fireEvent.click(screen.getByText("Open assistant"));
    const input = await screen.findByLabelText("Message");
    fireEvent.change(input, { target: { value: "Butter substitute?" } });
    fireEvent.keyDown(input, { key: "Enter" });
    await screen.findByText("Use olive oil.");

    // Act
    fireEvent.click(screen.getByRole("button", { name: "New chat" }));

    // Assert
    expect(screen.queryByText("Use olive oil.")).not.toBeInTheDocument();
    expect(screen.getByText("Nothing yet")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "New chat" })).not.toBeInTheDocument();
  });

  it("WhenErrorBeforeReply_ShouldRestoreTypedMessage", async () => {
    // Arrange
    vi.mocked(streamAssistant).mockReturnValueOnce(
      events({ $type: "error", message: "The assistant failed." }),
    );
    renderPanel();
    fireEvent.click(screen.getByText("Open assistant"));
    const input = await screen.findByLabelText("Message");

    // Act
    fireEvent.change(input, { target: { value: "Butter substitute?" } });
    fireEvent.keyDown(input, { key: "Enter" });

    // Assert
    expect(await screen.findByRole("alert")).toHaveTextContent("The assistant failed.");
    expect(input).toHaveValue("Butter substitute?");
    expect(screen.getByText("Nothing yet")).toBeInTheDocument();
  });

  it("WhenOpenedOnDesktop_ShouldFocusMessageInput", async () => {
    // Arrange
    renderPanel();

    // Act
    fireEvent.click(screen.getByText("Open assistant"));

    // Assert
    expect(await screen.findByLabelText("Message")).toHaveFocus();
  });

  it("WhenMobile_ShouldRenderDrawer", async () => {
    // Arrange
    vi.mocked(useIsMobile).mockReturnValue(true);
    renderPanel();

    // Act
    fireEvent.click(screen.getByText("Open assistant"));

    // Assert
    expect(await screen.findByText("Test Assistant")).toBeInTheDocument();
    expect(document.querySelector("[data-slot='drawer-content']")).not.toBeNull();
  });
});
