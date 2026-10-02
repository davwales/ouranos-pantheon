import { describe, it, expect } from "vitest";
import { abbreviateNumber } from "./abbreviate-number";

describe("abbreviateNumber", () => {
  it.each([
    [0, "0"],
    [500, "500.00"],
    [1500, "1.50K"],
    [2500000, "2.50M"],
    [1750000000, "1.75B"],
    [2147483647000, "2.15T"],
    [1000000000000000, "1.00Q"],
    [2500000000000000, "2.50Q"],
    [-1500, "-1.50K"],
  ])("abbreviates %p as %p", (input, expected) => {
    expect(abbreviateNumber(input)).toBe(expected);
  });

  it("honours the decimals parameter", () => {
    expect(abbreviateNumber(2147483647000, 1)).toBe("2.1T");
  });
});