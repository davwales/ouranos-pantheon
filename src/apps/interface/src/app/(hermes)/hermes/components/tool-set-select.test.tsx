import { ToolSetSelect } from "@/app/(hermes)/hermes/components/tool-set-select";
import { type ToolSetSummary } from "@/lib/api/hermes";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

const roleplay: ToolSetSummary = {
  id: "set-1",
  name: "Roleplay",
  description: null,
  tools: [],
  isPublic: true,
};

async function choose(name: string) {
  fireEvent.click(screen.getByRole("combobox", { name: "Tool set" }));
  fireEvent.click(await screen.findByRole("option", { name }));
}

describe("ToolSetSelect", () => {
  it("WhenAToolSetIsChosen_ShouldReportItsId", async () => {
    // Arrange
    const onChange = vi.fn();
    render(<ToolSetSelect value={null} toolSets={[roleplay]} onChange={onChange} />);

    // Act
    await choose("Roleplay");

    // Assert
    expect(onChange).toHaveBeenCalledWith("set-1");
  });

  it("WhenNoToolsIsChosen_ShouldReportNull", async () => {
    // Arrange
    const onChange = vi.fn();
    render(
      <ToolSetSelect value="set-1" toolSets={[roleplay]} onChange={onChange} />,
    );

    // Act
    await choose("No tools");

    // Assert
    expect(onChange).toHaveBeenCalledWith(null);
  });
});
