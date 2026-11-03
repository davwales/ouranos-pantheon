import { buildBreadcrumbTrail } from "@/components/shared/breadcrumbs/breadcrumb-trail";
import { describe, expect, it } from "vitest";

const MARKET_ID = "1b2f3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";
const SYMBOL_ID = "3b2f3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";
const GROUP_ID = "4b2f3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";
const SYMBOL_PATH = `/plutus/${MARKET_ID}/${SYMBOL_ID}`;

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

  it("WhenOriginIsSiblingList_ShouldInsertOriginBeforeCurrentPage", () => {
    // Arrange & Act
    const crumbs = buildBreadcrumbTrail(SYMBOL_PATH, `/plutus/${MARKET_ID}/explorer`);

    // Assert
    expect(crumbs.map((crumb) => crumb.href)).toEqual([
      "/plutus",
      `/plutus/${MARKET_ID}`,
      `/plutus/${MARKET_ID}/explorer`,
      SYMBOL_PATH,
    ]);
    expect(crumbs[2].label).toBe("Explorer");
  });

  it("WhenOriginEntityLabelled_ShouldInsertOriginTrail", () => {
    // Arrange
    const origin = `/plutus/${MARKET_ID}/groups/${GROUP_ID}`;

    // Act
    const crumbs = buildBreadcrumbTrail(SYMBOL_PATH, origin, { [GROUP_ID]: "Herbs" });

    // Assert
    expect(crumbs.map((crumb) => crumb.segment)).toEqual([
      "plutus",
      MARKET_ID,
      "groups",
      GROUP_ID,
      SYMBOL_ID,
    ]);
  });

  it("WhenOriginEntityUnlabelled_ShouldFallBackToPathTrail", () => {
    // Arrange & Act
    const crumbs = buildBreadcrumbTrail(SYMBOL_PATH, `/plutus/${MARKET_ID}/groups/${GROUP_ID}`);

    // Assert
    expect(crumbs).toEqual(buildBreadcrumbTrail(SYMBOL_PATH));
  });

  it.each([
    ["outside the parent", "/hestia/recipes"],
    ["the page itself", SYMBOL_PATH],
    ["an external URL", "//example.com/plutus"],
    ["a path with a query", `/plutus/${MARKET_ID}/explorer?x=1`],
  ])("WhenOriginIs%s_ShouldIgnoreIt", (_, origin) => {
    // Arrange & Act
    const crumbs = buildBreadcrumbTrail(SYMBOL_PATH, origin);

    // Assert
    expect(crumbs).toEqual(buildBreadcrumbTrail(SYMBOL_PATH));
  });
});
