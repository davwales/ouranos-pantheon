import { cloneElement } from "react";
import type { ReactElement } from "react";
import { describe, expect, it, vi } from "vitest";
import { render } from "@testing-library/react";
import { Sparkline, resolveValueColor } from "@/components/shared/sparkline";

vi.mock("recharts", async (importOriginal) => {
  const actual = await importOriginal<typeof import("recharts")>();
  return {
    ...actual,
    ResponsiveContainer: ({ children }: { children: ReactElement }) =>
      cloneElement(children, { width: 400, height: 48 }),
  };
});

describe("Sparkline", () => {
  it("renders nothing when there are fewer than two points", () => {
    const { container } = render(
      <Sparkline data={[{ value: 1, timestamp: "2026-01-01T00:00:00Z" }]} />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  it("renders gradient stops with the configured colors", () => {
    const { container } = render(
      <Sparkline
        data={[
          { value: -1, timestamp: "2026-01-01T00:00:00Z" },
          { value: 1, timestamp: "2026-01-02T00:00:00Z" },
        ]}
        colorPositive="#111111"
        colorZero="#222222"
        colorNegative="#333333"
      />,
    );

    const stops = container.querySelectorAll("stop");
    expect(stops).toHaveLength(3);
    expect(stops[0]).toHaveAttribute("offset", "0%");
    expect(stops[0]).toHaveAttribute("stop-color", "#111111");
    expect(stops[1]).toHaveAttribute("offset", "50%");
    expect(stops[1]).toHaveAttribute("stop-color", "#222222");
    expect(stops[2]).toHaveAttribute("offset", "100%");
    expect(stops[2]).toHaveAttribute("stop-color", "#333333");
  });

  it("renders a zero reference line with two or more points", () => {
    const { container } = render(
      <Sparkline
        data={[
          { value: -1, timestamp: "2026-01-01T00:00:00Z" },
          { value: 1, timestamp: "2026-01-02T00:00:00Z" },
        ]}
      />,
    );

    expect(
      container.querySelector("line.recharts-reference-line-line"),
    ).toBeInTheDocument();
  });
});

describe("resolveValueColor", () => {
  it("returns the positive color for positive values", () => {
    expect(resolveValueColor(0.5, "green", "neutral", "red")).toBe("green");
  });

  it("returns the zero color for zero values", () => {
    expect(resolveValueColor(0, "green", "neutral", "red")).toBe("neutral");
  });

  it("returns the negative color for negative values", () => {
    expect(resolveValueColor(-0.5, "green", "neutral", "red")).toBe("red");
  });
});
