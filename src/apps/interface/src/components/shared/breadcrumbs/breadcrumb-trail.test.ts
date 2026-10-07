import { buildBreadcrumbTrail } from "@/components/shared/breadcrumbs/breadcrumb-trail";
import { describe, expect, it } from "vitest";

const MARKET_ID = "1b2f3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";

describe("buildBreadcrumbTrail", () => {
  it("WhenPathIsRoot_ShouldReturnEmptyTrail", () => {
    // Arrange & Act
    const crumbs = buildBreadcrumbTrail("/");

    // Assert
    expect(crumbs).toEqual([]);
  });

  it("WhenStaticSegments_ShouldTitleCaseFolderNames", () => {
    // Arrange & Act
    const crumbs = buildBreadcrumbTrail("/hestia/shopping-list");

    // Assert
    expect(crumbs).toEqual([
      { href: "/hestia", segment: "hestia", label: "Hestia" },
      { href: "/hestia/shopping-list", segment: "shopping-list", label: "Shopping List" },
    ]);
  });

  it("WhenGuidSegment_ShouldLeaveLabelForOwnerToRegister", () => {
    // Arrange & Act
    const crumbs = buildBreadcrumbTrail(`/plutus/${MARKET_ID}/strategies`);

    // Assert
    expect(crumbs).toEqual([
      { href: "/plutus", segment: "plutus", label: "Plutus" },
      { href: `/plutus/${MARKET_ID}`, segment: MARKET_ID, label: undefined },
      { href: `/plutus/${MARKET_ID}/strategies`, segment: "strategies", label: "Strategies" },
    ]);
  });
});
