import { usePublicMode } from "@/app/(hermes)/hermes/components/use-public-mode";
import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const useFeatureFlag = vi.hoisted(() => vi.fn());

vi.mock("@/components/shared/feature-flags-context", () => ({
  useFeatureFlag,
}));

describe("usePublicMode", () => {
  it("ShouldReadTheHermesFlagAndFailClosed", () => {
    // Arrange
    useFeatureFlag.mockReturnValue(true);

    // Act
    const { result } = renderHook(() => usePublicMode());

    // Assert
    expect(result.current).toBe(true);
    expect(useFeatureFlag).toHaveBeenCalledWith("hermes.public_mode", {
      whileUnknown: true,
    });
  });
});
