import { Breadcrumbs } from "@/components/shared/breadcrumbs/breadcrumbs";
import { useBreadcrumbStore } from "@/stores/breadcrumb-store";
import { act, fireEvent, render, screen, within } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const MARKET_ID = "1b2f3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";
const STRATEGY_ID = "2b2f3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";

const mockUsePathname = vi.fn();

vi.mock("next/navigation", () => ({
  usePathname: () => mockUsePathname(),
}));

describe("Breadcrumbs", () => {
  beforeEach(() => {
    useBreadcrumbStore.setState({ labels: {} });
  });

  it("WhenTrailShorterThanTwo_ShouldRenderNothing", () => {
    // Arrange
    mockUsePathname.mockReturnValue("/plutus");

    // Act
    const { container } = render(<Breadcrumbs />);

    // Assert
    expect(container).toBeEmptyDOMElement();
  });

  it("WhenStaticTrail_ShouldLinkAncestorsAndMarkCurrentPage", () => {
    // Arrange
    mockUsePathname.mockReturnValue("/hermes/chat");

    // Act
    render(<Breadcrumbs />);

    // Assert
    expect(screen.getByRole("link", { name: "Hermes" })).toHaveAttribute("href", "/hermes");
    const current = screen.getByText("Chat");
    expect(current).toHaveAttribute("aria-current", "page");
    expect(current).toHaveClass("font-medium");
  });

  it("WhenGuidLabelRegistered_ShouldShowRegisteredName", () => {
    // Arrange
    mockUsePathname.mockReturnValue(`/plutus/${MARKET_ID}/strategies`);
    useBreadcrumbStore.setState({ labels: { [MARKET_ID]: "Sandbox Market" } });

    // Act
    render(<Breadcrumbs />);

    // Assert
    expect(screen.getByRole("link", { name: "Sandbox Market" })).toHaveAttribute(
      "href",
      `/plutus/${MARKET_ID}`,
    );
  });

  it("WhenGuidLabelUnregistered_ShouldShowSkeleton", () => {
    // Arrange
    mockUsePathname.mockReturnValue(`/plutus/${MARKET_ID}/strategies`);

    // Act
    render(<Breadcrumbs />);

    // Assert
    const marketLink = screen.getByRole("link", { name: "Loading" });
    expect(marketLink).toHaveAttribute("href", `/plutus/${MARKET_ID}`);
    expect(marketLink.querySelector('[data-slot="skeleton"]')).not.toBeNull();
  });

  it("WhenLabelRegisteredAfterRender_ShouldUpdate", () => {
    // Arrange
    mockUsePathname.mockReturnValue(`/plutus/${MARKET_ID}/strategies`);
    render(<Breadcrumbs />);

    // Act
    act(() => useBreadcrumbStore.getState().setLabel(MARKET_ID, "Sandbox Market"));

    // Assert
    expect(screen.getByRole("link", { name: "Sandbox Market" })).toBeInTheDocument();
  });

  it("WhenDeepTrail_ShouldHideEarlyCrumbsOnMobileOnly", () => {
    // Arrange
    mockUsePathname.mockReturnValue(
      `/plutus/${MARKET_ID}/strategies/${STRATEGY_ID}/backtests`,
    );

    // Act
    render(<Breadcrumbs />);

    // Assert
    const items = within(screen.getByRole("navigation", { name: "Breadcrumb" })).getAllByRole(
      "listitem",
    );
    expect(items[0]).toHaveClass("md:hidden");
    const crumbItems = items.slice(1);
    expect(crumbItems).toHaveLength(5);
    expect(crumbItems.slice(0, 3).every((item) => item.classList.contains("hidden"))).toBe(true);
    expect(crumbItems.slice(3).some((item) => item.classList.contains("hidden"))).toBe(false);
  });

  it("WhenOverflowMenuOpened_ShouldLinkHiddenCrumbs", async () => {
    // Arrange
    mockUsePathname.mockReturnValue(`/plutus/${MARKET_ID}/strategies/${STRATEGY_ID}`);
    useBreadcrumbStore.setState({ labels: { [MARKET_ID]: "Sandbox Market" } });
    render(<Breadcrumbs />);

    // Act
    fireEvent.keyDown(screen.getByRole("button", { name: "Show full path" }), { key: "Enter" });

    // Assert
    const menu = await screen.findByRole("menu");
    expect(within(menu).getByRole("menuitem", { name: "Plutus" })).toHaveAttribute(
      "href",
      "/plutus",
    );
    expect(within(menu).getByRole("menuitem", { name: "Sandbox Market" })).toHaveAttribute(
      "href",
      `/plutus/${MARKET_ID}`,
    );
  });
});
