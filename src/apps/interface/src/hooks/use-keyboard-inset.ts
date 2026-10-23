import { useSyncExternalStore } from "react";

export type KeyboardInset = {
  /** Layout-viewport pixels below the visible area (under the keyboard). */
  bottom: number;
  /** Height of the area the user can actually see. */
  height: number;
};

let snapshot: KeyboardInset | null = null;

function getSnapshot(): KeyboardInset | null {
  const viewport = window.visualViewport;
  // Pinch-zoom also shrinks the visual viewport; only react to the on-screen keyboard.
  if (
    !viewport ||
    viewport.scale !== 1 ||
    window.innerHeight - viewport.height < 1
  ) {
    snapshot = null;
    return snapshot;
  }
  // iOS Safari keeps the layout viewport full height under the keyboard and may also pan the
  // visual viewport (offsetTop) to reveal the focused field, so both must be accounted for.
  const bottom = Math.max(
    0,
    window.innerHeight - viewport.height - viewport.offsetTop,
  );
  if (snapshot?.bottom !== bottom || snapshot.height !== viewport.height) {
    snapshot = { bottom, height: viewport.height };
  }
  return snapshot;
}

function subscribe(callback: () => void) {
  const viewport = window.visualViewport;
  viewport?.addEventListener("resize", callback);
  viewport?.addEventListener("scroll", callback);
  return () => {
    viewport?.removeEventListener("resize", callback);
    viewport?.removeEventListener("scroll", callback);
  };
}

function subscribeNever() {
  return () => {};
}

/**
 * Tracks the part of the layout viewport hidden by the on-screen keyboard, so fixed-position
 * UI (e.g. a bottom drawer) can sit directly above it. Returns null while the keyboard is closed.
 */
export function useKeyboardInset(enabled = true): KeyboardInset | null {
  return useSyncExternalStore(
    enabled ? subscribe : subscribeNever,
    enabled ? getSnapshot : () => null,
    () => null,
  );
}
