import { OriginLink } from "@/components/shared/breadcrumbs/origin-link";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

vi.mock("next/navigation", () => ({
  usePathname: () => "/plutus/market-1/explorer",
}));

describe("OriginLink", () => {
  it("WhenRendered_ShouldCarryCurrentPathAsOrigin", () => {
    // Arrange & Act
    render(<OriginLink href="/plutus/market-1/symbol-1">Iron ore</OriginLink>);

    // Assert
    const href = screen.getByRole("link", { name: "Iron ore" }).getAttribute("href") ?? "";
    const url = new URL(href, "http://localhost");
    expect(url.pathname).toBe("/plutus/market-1/symbol-1");
    expect(url.searchParams.get("from")).toBe("/plutus/market-1/explorer");
  });
});
