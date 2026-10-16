import { ActionBlock } from "@/components/shared/ai-assistant/blocks/action-block";
import { type AssistantChatActionPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const ACTION: AssistantChatActionPart = {
  kind: "action",
  id: "a1",
  callId: "c1",
  name: "add_note",
  action: { text: "Rest it" },
  status: "proposed",
  arguments: '{"text":"Rest it"}',
  output: "Proposed to the user.",
};

const HANDLER = {
  describe: (value: unknown) => `Add note: ${(value as { text: string }).text}`,
  apply: vi.fn(),
};

function renderAction(update: Partial<AssistantChatActionPart> = {}, handler = HANDLER) {
  const onApply = vi.fn();
  const onReject = vi.fn();
  render(
    <ActionBlock
      action={{ ...ACTION, ...update }}
      handler={handler}
      onApply={onApply}
      onReject={onReject}
    />,
  );
  return { onApply, onReject };
}

describe("ActionBlock", () => {
  it("WhenProposed_ShouldDescribeItAndOfferApplyAndReject", () => {
    // Arrange
    const { onApply, onReject } = renderAction();

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Apply" }));
    fireEvent.click(screen.getByRole("button", { name: "Reject" }));

    // Assert
    expect(screen.getByText("Proposed change")).toBeInTheDocument();
    expect(screen.getByText("Add note: Rest it")).toBeInTheDocument();
    expect(onApply).toHaveBeenCalledOnce();
    expect(onReject).toHaveBeenCalledOnce();
  });

  it("WhenDetailsAreOpened_ShouldShowTheToolInputAndOutput", () => {
    // Arrange
    renderAction();
    expect(screen.queryByText("Proposed to the user.")).not.toBeInTheDocument();

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Details" }));

    // Assert
    expect(screen.getByText(/"text": "Rest it"/)).toBeInTheDocument();
    expect(screen.getByText("Proposed to the user.")).toBeInTheDocument();
  });

  it("WhenGivenTheTurnContext_ShouldShowItInTheDetailsInput", () => {
    // Arrange
    render(
      <ActionBlock
        action={ACTION}
        handler={HANDLER}
        context={{ recipeId: "r1" }}
        onApply={vi.fn()}
        onReject={vi.fn()}
      />,
    );

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Details" }));

    // Assert
    const input = screen.getByRole("region", { name: "Input" }).querySelector("pre");
    expect(JSON.parse(input?.textContent ?? "")).toEqual({
      context: { recipeId: "r1" },
      arguments: { text: "Rest it" },
    });
  });

  it("WhenApplying_ShouldDisableBothButtons", () => {
    // Arrange & Act
    renderAction({ status: "applying" });

    // Assert
    expect(screen.getByRole("button", { name: "Apply" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Reject" })).toBeDisabled();
  });

  it.each(["applied", "rejected"] as const)("WhenSettledAs%s_ShouldHideTheButtons", (status) => {
    // Arrange & Act
    renderAction({ status });

    // Assert
    expect(screen.getByText(status === "applied" ? "Applied" : "Rejected")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Apply" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Reject" })).not.toBeInTheDocument();
  });

  it("WhenFailed_ShouldShowTheErrorAndOfferRetryAndReject", () => {
    // Arrange
    const { onApply } = renderAction({ status: "failed", error: "Recipe changed" });

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Retry" }));

    // Assert
    expect(screen.getByRole("alert")).toHaveTextContent("Recipe changed");
    expect(screen.getByRole("button", { name: "Reject" })).toBeInTheDocument();
    expect(onApply).toHaveBeenCalledOnce();
  });

  it("WhenThePageHasNoHandler_ShouldSayItCannotBeApplied", () => {
    // Arrange & Act
    render(
      <ActionBlock action={ACTION} handler={undefined} onApply={vi.fn()} onReject={vi.fn()} />,
    );

    // Assert
    expect(screen.getByText("This page can’t apply this change.")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Apply" })).not.toBeInTheDocument();
  });
});
