import { MarkdownRenderer } from "@/components/shared/markdown-renderer";
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

const TABLE = `| Window | Low | High |
|---|---|---|
| OneDay | 188 | 195 |`;

describe("MarkdownRenderer", () => {
  it("WhenContentHasTable_ShouldRenderItInsideScrollableContainer", () => {
    // Arrange & Act
    render(<MarkdownRenderer variant="compact">{TABLE}</MarkdownRenderer>);

    // Assert
    const table = screen.getByRole("table");
    expect(table.parentElement).toHaveAttribute("data-slot", "table-container");
    expect(table.parentElement).toHaveClass("overflow-x-auto");
    expect(screen.getByRole("columnheader", { name: "Window" })).toBeInTheDocument();
    expect(screen.getByRole("cell", { name: "188" })).toBeInTheDocument();
  });

  it("WhenCompact_ShouldRenderChatSizedHeadings", () => {
    // Arrange & Act
    render(
      <MarkdownRenderer variant="compact">{"# Title\n\n### Detail"}</MarkdownRenderer>,
    );

    // Assert
    const title = screen.getByRole("heading", { level: 1, name: "Title" });
    expect(title).toHaveClass("text-base");
    expect(title).not.toHaveClass("text-4xl");
    expect(screen.getByRole("heading", { level: 3, name: "Detail" })).toHaveClass(
      "text-sm",
    );
  });

  it("WhenDefault_ShouldRenderPageSizedHeadings", () => {
    // Arrange & Act
    render(<MarkdownRenderer>{"# Title"}</MarkdownRenderer>);

    // Assert
    expect(screen.getByRole("heading", { level: 1, name: "Title" })).toHaveClass(
      "text-4xl",
    );
  });

  it("WhenContentHasRule_ShouldRenderSeparator", () => {
    // Arrange & Act
    render(<MarkdownRenderer variant="compact">{"Above\n\n---\n\nBelow"}</MarkdownRenderer>);

    // Assert
    expect(screen.getByRole("separator")).toBeInTheDocument();
  });

  it("WhenContentHasLink_ShouldOpenInNewTab", () => {
    // Arrange & Act
    render(
      <MarkdownRenderer variant="compact">
        {"[Wiki](https://oldschool.runescape.wiki)"}
      </MarkdownRenderer>,
    );

    // Assert
    const link = screen.getByRole("link", { name: "Wiki" });
    expect(link).toHaveAttribute("target", "_blank");
    expect(link).toHaveAttribute("rel", "noreferrer");
  });
});
