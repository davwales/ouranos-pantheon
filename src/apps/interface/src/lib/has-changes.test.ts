import { describe, expect, it } from "vitest";
import { hasChanges } from "./has-changes";

describe("hasChanges", () => {
  it("returns false for structurally equal values", () => {
    expect(
      hasChanges({ name: "a", tags: [1, 2] }, { name: "a", tags: [1, 2] }),
    ).toBe(false);
  });

  it("returns true when a nested value differs", () => {
    expect(
      hasChanges({ name: "a", tags: [1, 2] }, { name: "a", tags: [2, 1] }),
    ).toBe(true);
  });

  it("treats undefined and null differently", () => {
    expect(hasChanges({ notes: null }, {})).toBe(true);
  });
});
