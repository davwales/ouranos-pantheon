import { describe, expect, it } from "vitest";
import { isPaletteShortcut } from "./is-palette-shortcut";

describe("isPaletteShortcut", () => {
  it.each([
    [{ key: "k", metaKey: true }, true],
    [{ key: "K", ctrlKey: true }, true],
    [{ key: "k" }, false],
    [{ key: "k", ctrlKey: true, shiftKey: true }, false],
    [{ key: "k", metaKey: true, altKey: true }, false],
    [{ key: "j", metaKey: true }, false],
  ])("returns the expected result for %o", (init, expected) => {
    expect(isPaletteShortcut(new KeyboardEvent("keydown", init))).toBe(
      expected,
    );
  });
});
