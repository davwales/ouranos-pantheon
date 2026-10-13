import { type FlipConfiguration, type SwingConfiguration } from "@/lib/api/plutus";
import { describe, expect, it } from "vitest";
import { DEFAULT_FLIP_CONFIGURATION } from "./flip-constants";
import {
  isFullyLocked,
  isLocked,
  lockedFields,
  tunableFields,
  withLock,
  withValidLocks,
} from "./parameter-locks";

const SWING: SwingConfiguration = {
  mode: "Swing",
  inputWeights: [
    { kind: "SignalRsi", weight: 0.5 },
    { kind: "SignalPriceVelocity", weight: 0 },
  ],
  buyThreshold: 0.2,
  sellThreshold: null,
  maxPositions: 8,
  maxPositionPercent: 0.1,
  holdPeriodDays: null,
};

describe("parameter-locks", () => {
  it("withLock_ShouldToggleAKeyWithoutDuplicating", () => {
    // Arrange
    const locked = withLock(withLock(SWING, "maxPositions", true), "maxPositions", true);

    // Act
    const unlocked = withLock(locked, "maxPositions", false);

    // Assert
    expect(locked.lockedParameters).toEqual(["maxPositions"]);
    expect(isLocked(locked, "maxPositions")).toBe(true);
    expect(isLocked(unlocked, "maxPositions")).toBe(false);
  });

  it("tunableFields_WhenFlip_ShouldExcludeNeverOptimizedFields", () => {
    // Act
    const keys = tunableFields(DEFAULT_FLIP_CONFIGURATION).map((f) => f.key);

    // Assert
    expect(keys).not.toContain("minPrice");
    expect(keys).not.toContain("maxOpenOrders");
    expect(keys).toContain("bidOffset");
  });

  it("withValidLocks_ShouldDropLocksOnUnsetFieldsAndUnusedInputs", () => {
    // Arrange
    const configuration: SwingConfiguration = {
      ...SWING,
      lockedParameters: [
        "maxPositions",
        "holdPeriodDays",
        "weight.SignalRsi",
        "weight.SignalPriceVelocity",
      ],
    };

    // Act
    const result = withValidLocks(configuration);

    // Assert
    expect(result.lockedParameters).toEqual(["maxPositions", "weight.SignalRsi"]);
    expect(lockedFields(result).map((f) => f.value)).toEqual([8, 0.5]);
  });

  it("isFullyLocked_ShouldBeTrueOnlyWhenEveryTunableFieldIsLocked", () => {
    // Arrange
    const all = tunableFields(DEFAULT_FLIP_CONFIGURATION).map((f) => f.key);
    const configuration: FlipConfiguration = {
      ...DEFAULT_FLIP_CONFIGURATION,
      lockedParameters: all,
    };

    // Act & Assert
    expect(isFullyLocked(configuration)).toBe(true);
    expect(isFullyLocked(withLock(configuration, "bidOffset", false))).toBe(false);
  });
});
