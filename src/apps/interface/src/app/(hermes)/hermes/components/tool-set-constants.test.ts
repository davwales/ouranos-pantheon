import {
  GENERATE_IMAGE_TOOL,
  toolSetEnables,
} from "@/app/(hermes)/hermes/components/tool-set-constants";
import { type ToolSetSummary } from "@/lib/api/hermes";
import { describe, expect, it } from "vitest";

function toolSet(isAvailable: boolean): ToolSetSummary {
  return {
    id: "set-1",
    name: "Roleplay",
    description: null,
    isPublic: true,
    tools: [
      {
        source: "Native",
        name: GENERATE_IMAGE_TOOL,
        additionalDescription: null,
        isAvailable,
      },
    ],
  };
}

describe("toolSetEnables", () => {
  it("WhenTheSetEnablesTheAvailableTool_ShouldReturnTrue", () => {
    // Act
    const result = toolSetEnables(toolSet(true), GENERATE_IMAGE_TOOL);

    // Assert
    expect(result).toBe(true);
  });

  it("WhenTheToolIsUnavailable_ShouldReturnFalse", () => {
    // Act
    const result = toolSetEnables(toolSet(false), GENERATE_IMAGE_TOOL);

    // Assert
    expect(result).toBe(false);
  });

  it("WhenThereIsNoActiveSet_ShouldReturnFalse", () => {
    // Act
    const result = toolSetEnables(undefined, GENERATE_IMAGE_TOOL);

    // Assert
    expect(result).toBe(false);
  });
});
