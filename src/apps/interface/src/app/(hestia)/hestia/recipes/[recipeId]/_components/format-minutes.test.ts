import { describe, expect, it } from "vitest";
import { formatMinutes } from "./format-minutes";

describe("formatMinutes", () => {
  it.each([
    [0, "0 min"],
    [45, "45 min"],
    [60, "1 hr"],
    [75, "1 hr 15 min"],
    [1440, "24 hr"],
  ])("formats %i minutes as %s", (minutes, expected) => {
    expect(formatMinutes(minutes)).toBe(expected);
  });
});
