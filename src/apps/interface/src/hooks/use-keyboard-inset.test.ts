import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it } from "vitest";
import { useKeyboardInset } from "./use-keyboard-inset";

const INNER_HEIGHT = 660;

class FakeVisualViewport extends EventTarget {
  height = INNER_HEIGHT;
  offsetTop = 0;
  scale = 1;

  set(values: Partial<Pick<FakeVisualViewport, "height" | "offsetTop" | "scale">>) {
    Object.assign(this, values);
    this.dispatchEvent(new Event("resize"));
  }
}

describe("useKeyboardInset", () => {
  let viewport: FakeVisualViewport;
  const original = Object.getOwnPropertyDescriptor(window, "visualViewport");

  beforeEach(() => {
    viewport = new FakeVisualViewport();
    Object.defineProperty(window, "visualViewport", {
      configurable: true,
      value: viewport,
    });
    Object.defineProperty(window, "innerHeight", {
      configurable: true,
      value: INNER_HEIGHT,
    });
  });

  afterEach(() => {
    if (original) {
      Object.defineProperty(window, "visualViewport", original);
    } else {
      delete (window as { visualViewport?: unknown }).visualViewport;
    }
  });

  it("WhenKeyboardClosed_ShouldReturnNull", () => {
    // Act
    const { result } = renderHook(() => useKeyboardInset());

    // Assert
    expect(result.current).toBeNull();
  });

  it("WhenKeyboardOpens_ShouldReturnCoveredHeight", () => {
    // Arrange
    const { result } = renderHook(() => useKeyboardInset());

    // Act
    act(() => viewport.set({ height: 320 }));

    // Assert
    expect(result.current).toEqual({ bottom: 340, height: 320 });
  });

  it("WhenViewportPannedUnderKeyboard_ShouldSubtractPan", () => {
    // Arrange
    const { result } = renderHook(() => useKeyboardInset());

    // Act
    act(() => viewport.set({ height: 320, offsetTop: 330 }));

    // Assert
    expect(result.current).toEqual({ bottom: 10, height: 320 });
  });

  it("WhenKeyboardCloses_ShouldReturnNull", () => {
    // Arrange
    const { result } = renderHook(() => useKeyboardInset());
    act(() => viewport.set({ height: 320, offsetTop: 330 }));

    // Act
    act(() => viewport.set({ height: INNER_HEIGHT, offsetTop: 0 }));

    // Assert
    expect(result.current).toBeNull();
  });

  it("WhenPinchZoomed_ShouldReturnNull", () => {
    // Arrange
    const { result } = renderHook(() => useKeyboardInset());

    // Act
    act(() => viewport.set({ height: 330, scale: 2 }));

    // Assert
    expect(result.current).toBeNull();
  });

  it("WhenDisabled_ShouldIgnoreKeyboard", () => {
    // Arrange
    const { result } = renderHook(() => useKeyboardInset(false));

    // Act
    act(() => viewport.set({ height: 320 }));

    // Assert
    expect(result.current).toBeNull();
  });
});
