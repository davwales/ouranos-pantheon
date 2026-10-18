import { type ToolSetSummary, type ToolSource } from "@/lib/api/hermes";

export const MAX_TOOL_SET_INSTRUCTIONS_LENGTH = 1000;
export const MAX_TOOL_ADDITIONAL_DESCRIPTION_LENGTH = 500;

export const TOOL_SOURCE_LABELS: Record<ToolSource, string> = {
  Native: "Native",
};

export function toolKey(tool: { source: ToolSource; name: string }): string {
  return `${tool.source}:${tool.name}`;
}

export const GENERATE_IMAGE_TOOL = "generate_image";

export function toolSetEnables(
  toolSet: ToolSetSummary | undefined,
  name: string,
): boolean {
  return (
    toolSet?.tools.some(
      (t) => t.source === "Native" && t.name === name && t.isAvailable,
    ) ?? false
  );
}
