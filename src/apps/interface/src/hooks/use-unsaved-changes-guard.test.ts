import { renderHook } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { useUnsavedChangesGuard } from "./use-unsaved-changes-guard";

function dispatchBeforeUnload(): Event {
  const event = new Event("beforeunload", { cancelable: true });
  window.dispatchEvent(event);
  return event;
}

describe("useUnsavedChangesGuard", () => {
  it("does not block unload when clean", () => {
    renderHook(() => useUnsavedChangesGuard(false));

    expect(dispatchBeforeUnload().defaultPrevented).toBe(false);
  });

  it("blocks unload while dirty", () => {
    renderHook(() => useUnsavedChangesGuard(true));

    expect(dispatchBeforeUnload().defaultPrevented).toBe(true);
  });

  it("stops blocking once changes are saved", () => {
    const { rerender } = renderHook(
      ({ isDirty }) => useUnsavedChangesGuard(isDirty),
      { initialProps: { isDirty: true } },
    );

    rerender({ isDirty: false });

    expect(dispatchBeforeUnload().defaultPrevented).toBe(false);
  });

  it("stops blocking after unmount", () => {
    const { unmount } = renderHook(() => useUnsavedChangesGuard(true));

    unmount();

    expect(dispatchBeforeUnload().defaultPrevented).toBe(false);
  });
});
