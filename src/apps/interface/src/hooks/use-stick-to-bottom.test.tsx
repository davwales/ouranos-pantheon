import { act, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it } from "vitest";
import { useStickToBottom } from "./use-stick-to-bottom";

const CLIENT_HEIGHT = 500;

let resizeCallbacks: (() => void)[] = [];

class FakeResizeObserver {
  constructor(private readonly callback: () => void) {}

  observe() {
    resizeCallbacks.push(this.callback);
  }

  unobserve() {}

  disconnect() {
    resizeCallbacks = resizeCallbacks.filter((callback) => callback !== this.callback);
  }
}

function Harness() {
  const { contentRef, isAtBottom, scrollToBottom } = useStickToBottom<HTMLDivElement>();
  return (
    <div data-testid="scroller" style={{ overflowY: "auto" }}>
      <div ref={contentRef} />
      <span data-testid="state">{isAtBottom ? "at-bottom" : "detached"}</span>
      <button onClick={scrollToBottom}>Jump</button>
    </div>
  );
}

function state() {
  return screen.getByTestId("state").textContent;
}

function setUp(scrollHeight: number) {
  render(<Harness />);
  const scroller = screen.getByTestId("scroller");
  Object.defineProperty(scroller, "clientHeight", { configurable: true, value: CLIENT_HEIGHT });
  setScrollHeight(scroller, scrollHeight);
  return scroller;
}

function setScrollHeight(scroller: HTMLElement, scrollHeight: number) {
  Object.defineProperty(scroller, "scrollHeight", { configurable: true, value: scrollHeight });
}

function grow(scroller: HTMLElement, scrollHeight: number) {
  setScrollHeight(scroller, scrollHeight);
  act(() => resizeCallbacks.forEach((callback) => callback()));
}

function userScrollTo(scroller: HTMLElement, top: number) {
  scroller.scrollTop = top;
  fireEvent.scroll(scroller);
}

describe("useStickToBottom", () => {
  const original = window.ResizeObserver;

  beforeEach(() => {
    resizeCallbacks = [];
    window.ResizeObserver = FakeResizeObserver as unknown as typeof ResizeObserver;
  });

  afterEach(() => {
    window.ResizeObserver = original;
  });

  it("WhenContentGrowsAtTheBottom_ShouldFollowIt", () => {
    // Arrange
    const scroller = setUp(1000);
    grow(scroller, 1000);

    // Act
    grow(scroller, 1400);

    // Assert
    expect(scroller.scrollTop).toBe(1400);
    expect(state()).toBe("at-bottom");
  });

  it("WhenTheUserScrollsUp_ShouldStopFollowing", () => {
    // Arrange
    const scroller = setUp(1000);
    grow(scroller, 1000);
    userScrollTo(scroller, 500);

    // Act
    userScrollTo(scroller, 200);
    grow(scroller, 1400);

    // Assert
    expect(scroller.scrollTop).toBe(200);
    expect(state()).toBe("detached");
  });

  it("WhenTheUserScrollsUpWithinTheThreshold_ShouldStillStopFollowing", () => {
    // Arrange
    const scroller = setUp(1000);
    grow(scroller, 1000);
    userScrollTo(scroller, 500);

    // Act
    userScrollTo(scroller, 490);
    grow(scroller, 1400);

    // Assert
    expect(scroller.scrollTop).toBe(490);
    expect(state()).toBe("detached");
  });

  it("WhenTheUserScrollsBackNearTheBottom_ShouldResumeFollowing", () => {
    // Arrange
    const scroller = setUp(1000);
    grow(scroller, 1000);
    userScrollTo(scroller, 200);

    // Act
    userScrollTo(scroller, 470);
    grow(scroller, 1400);

    // Assert
    expect(scroller.scrollTop).toBe(1400);
    expect(state()).toBe("at-bottom");
  });

  it("WhenScrollToBottomIsCalled_ShouldJumpAndResumeFollowing", () => {
    // Arrange
    const scroller = setUp(1000);
    userScrollTo(scroller, 500);
    userScrollTo(scroller, 100);

    // Act
    fireEvent.click(screen.getByRole("button", { name: "Jump" }));
    grow(scroller, 1600);

    // Assert
    expect(scroller.scrollTop).toBe(1600);
    expect(state()).toBe("at-bottom");
  });
});
