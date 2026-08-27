import { describe, it, expect } from "vitest";
import {
  SCALE_MAX,
  SCALE_MIN,
  SCALE_STEP,
  clampScale,
  formatQuantity,
  scaleQuantity,
} from "./scale-ingredients";

describe("formatQuantity", () => {
  it.each([
    [0.5, "½"],
    [0.25, "¼"],
    [1 / 3, "⅓"],
    [2 / 3, "⅔"],
    [0.75, "¾"],
    [1.5, "1½"],
    [2.25, "2¼"],
    [2.75, "2¾"],
    [1, "1"],
    [6, "6"],
    [0.37, "0.37"],
    [1.67, "1⅔"],
    [0.99, "1"],
    [1.99, "2"],
    [2.98, "2.98"],
    [2.5, "2½"],
    [0.66, "⅔"],
    [0.35, "⅓"],
    [0.2, "0.2"],
  ])("formats %p as %p", (input, expected) => {
    expect(formatQuantity(input)).toBe(expected);
  });

  it.each([0, -1, Number.NaN, Number.POSITIVE_INFINITY])(
    "returns an empty string for %p",
    (input) => {
      expect(formatQuantity(input)).toBe("");
    },
  );
});

describe("scaleQuantity", () => {
  it("multiplies the quantity by the scale", () => {
    expect(scaleQuantity(4, 1.5)).toBe(6);
    expect(scaleQuantity(1, 2)).toBe(2);
    expect(scaleQuantity(0.333, 2)).toBeCloseTo(0.666);
  });
});

describe("clampScale", () => {
  it("returns values within bounds unchanged", () => {
    expect(clampScale(1)).toBe(1);
    expect(clampScale(2.5)).toBe(2.5);
  });

  it("clamps values below the minimum and above the maximum", () => {
    expect(clampScale(0.25)).toBe(SCALE_MIN);
    expect(clampScale(8)).toBe(SCALE_MAX);
  });

  it("snaps values to the step", () => {
    expect(clampScale(1.3)).toBeCloseTo(1.5);
    expect(SCALE_STEP).toBe(0.5);
  });
});