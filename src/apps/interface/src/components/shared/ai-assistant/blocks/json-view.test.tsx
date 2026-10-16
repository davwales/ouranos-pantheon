import { JsonView } from "@/components/shared/ai-assistant/blocks/json-view";
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

describe("JsonView", () => {
  it("WhenValueIsJson_ShouldPrettyPrintIt", () => {
    // Arrange & Act
    render(<JsonView label="Input" value='{"days":30}' emptyText="No arguments" />);

    // Assert
    expect(screen.getByText(/"days": 30/)).toBeInTheDocument();
  });

  it("WhenValueIsNotJson_ShouldShowItAsText", () => {
    // Arrange & Act
    render(<JsonView label="Output" value="Skipped: too many calls" emptyText="No output" />);

    // Assert
    expect(screen.getByText("Skipped: too many calls")).toBeInTheDocument();
  });

  it.each(["", "  ", "{}", undefined])("WhenValueIsEmpty_ShouldShowTheEmptyText (%s)", (value) => {
    // Arrange & Act
    render(<JsonView label="Input" value={value} emptyText="No arguments" />);

    // Assert
    expect(screen.getByText("No arguments")).toBeInTheDocument();
  });

  it("WhenJsonWasCutShort_ShouldStillIndentIt", () => {
    // Arrange & Act
    const { container } = render(
      <JsonView label="Output" value={'{"rows":[{"name":"a, \\"b\\"","n":1},{"na…'} emptyText="No output" />,
    );

    // Assert
    expect(container.querySelector("pre")?.textContent).toBe(
      '{\n  "rows": [\n    {\n      "name": "a, \\"b\\"",\n      "n": 1\n    },\n    {\n      "na…',
    );
  });

  it("WhenGivenAFooter_ShouldShowIt", () => {
    // Arrange & Act
    render(<JsonView label="Output" value="abc…" emptyText="No output" footer="Truncated" />);

    // Assert
    expect(screen.getByText("Truncated")).toBeInTheDocument();
  });
});
