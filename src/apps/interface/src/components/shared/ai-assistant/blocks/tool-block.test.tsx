import { ToolBlock } from "@/components/shared/ai-assistant/blocks/tool-block";
import { type AssistantChatToolPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const TOOL: AssistantChatToolPart = {
  kind: "tool",
  id: "p1",
  callId: "c1",
  name: "get_price_history",
  arguments: '{"days":30}',
  status: "completed",
  output: '{"close":12}',
  isTruncated: false,
  startedAt: 1000,
  endedAt: 1400,
};

describe("ToolBlock", () => {
  it("WhenCollapsed_ShouldShowOnlyTheNameAndDuration", () => {
    // Arrange & Act
    render(<ToolBlock tool={TOOL} />);

    // Assert
    const header = screen.getByRole("button", { name: /get_price_history/ });
    expect(header).toHaveAttribute("aria-expanded", "false");
    expect(header).toHaveTextContent("0.4s");
    expect(screen.queryByText(/"days": 30/)).not.toBeInTheDocument();
  });

  it("WhenSaved_ShouldShowTheRecordedDuration", () => {
    // Arrange & Act
    render(
      <ToolBlock
        tool={{ ...TOOL, startedAt: undefined, endedAt: undefined, durationMs: 83_000 }}
      />,
    );

    // Assert
    expect(screen.getByRole("button", { name: /get_price_history/ })).toHaveTextContent("83s");
  });

  it("WhenSavedWithoutADuration_ShouldNotMakeOneUp", () => {
    // Arrange & Act
    render(<ToolBlock tool={{ ...TOOL, startedAt: undefined, endedAt: undefined }} />);

    // Assert
    const header = screen.getByRole("button", { name: /get_price_history/ });
    expect(header).toHaveTextContent("Completed");
    expect(header).not.toHaveTextContent(/\d+(\.\d)?s/);
  });

  it("WhenExpanded_ShouldShowTheInputAndOutput", () => {
    // Arrange
    render(<ToolBlock tool={TOOL} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /get_price_history/ }));

    // Assert
    expect(screen.getByText(/"days": 30/)).toBeInTheDocument();
    expect(screen.getByText(/"close": 12/)).toBeInTheDocument();
    expect(screen.queryByText(/Truncated/)).not.toBeInTheDocument();
  });

  it("WhenExpandedWithTheKeyboard_ShouldShowTheDetails", () => {
    // Arrange
    render(<ToolBlock tool={TOOL} />);

    // Act
    fireEvent.keyDown(screen.getByRole("button", { name: /get_price_history/ }), { key: "Enter" });

    // Assert
    expect(screen.getByText(/"days": 30/)).toBeInTheDocument();
  });

  it("WhenGivenTheTurnContext_ShouldShowItWithTheArgumentsAsTheInput", () => {
    // Arrange
    render(<ToolBlock tool={TOOL} context={{ marketId: "m1", symbolId: "s1" }} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /get_price_history/ }));

    // Assert
    const input = screen.getByRole("region", { name: "Input" }).querySelector("pre");
    expect(JSON.parse(input?.textContent ?? "")).toEqual({
      context: { marketId: "m1", symbolId: "s1" },
      arguments: { days: 30 },
    });
  });

  it("WhenGivenContextAndTheCallHasNoArguments_ShouldStillShowTheContext", () => {
    // Arrange
    render(<ToolBlock tool={{ ...TOOL, arguments: "" }} context={{ symbolId: "s1" }} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /get_price_history/ }));

    // Assert
    const input = screen.getByRole("region", { name: "Input" }).querySelector("pre");
    expect(JSON.parse(input?.textContent ?? "")).toEqual({
      context: { symbolId: "s1" },
      arguments: {},
    });
  });

  it("WhenArgumentsAreNotJson_ShouldShowThemRawBesideTheContext", () => {
    // Arrange
    render(<ToolBlock tool={{ ...TOOL, arguments: "not json" }} context={{ symbolId: "s1" }} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /get_price_history/ }));

    // Assert
    const input = screen.getByRole("region", { name: "Input" }).querySelector("pre");
    expect(JSON.parse(input?.textContent ?? "")).toEqual({
      context: { symbolId: "s1" },
      arguments: "not json",
    });
  });

  it("WhenOutputWasTruncated_ShouldSaySo", () => {
    // Arrange
    render(<ToolBlock tool={{ ...TOOL, output: "abc…", isTruncated: true }} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /get_price_history/ }));

    // Assert
    expect(screen.getByText("Truncated: the assistant only saw this much.")).toBeInTheDocument();
  });

  it("WhenStillRunning_ShouldSayItIsWaitingForTheResult", () => {
    // Arrange
    render(<ToolBlock tool={{ ...TOOL, status: "running", output: undefined, endedAt: undefined }} />);

    // Act
    fireEvent.click(screen.getByRole("button", { name: /get_price_history/ }));

    // Assert
    expect(screen.getByText(/^Running…/)).toBeInTheDocument();
    expect(screen.getByText("Waiting for the result…")).toBeInTheDocument();
  });

  it.each([
    ["failed", "Failed"],
    ["skipped", "Skipped"],
    ["cancelled", "Cancelled"],
  ] as const)("WhenStatusIs%s_ShouldLabelIt", (status, label) => {
    // Arrange & Act
    render(<ToolBlock tool={{ ...TOOL, status }} />);

    // Assert
    expect(screen.getByText(label)).toBeInTheDocument();
  });

  it("WhenRunning_ShouldShowTheElapsedTime", () => {
    // Arrange
    vi.useFakeTimers();
    vi.setSystemTime(10_000);

    // Act
    render(<ToolBlock tool={{ ...TOOL, status: "running", startedAt: 7_000, endedAt: undefined }} />);

    // Assert
    expect(screen.getByRole("button", { name: /get_price_history/ })).toHaveTextContent(
      "Running… 3s",
    );
    vi.useRealTimers();
  });
});
