import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { Typography } from "@/components/shared/typography";

describe("Typography", () => {
  it("renders children text", () => {
    render(<Typography>Hello</Typography>);
    expect(screen.getByText("Hello")).toBeInTheDocument();
  });
});