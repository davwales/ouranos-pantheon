import { describe, expect, it } from "vitest";
import { type Market } from "@/lib/api/plutus";
import { buildStaticGroups } from "./build-static-groups";

const markets: Market[] = [
  {
    id: "m1",
    name: "OSRS",
    description: "Old School",
  } as Market,
];

describe("buildStaticGroups", () => {
  it("returns actions, pages and markets in that order", () => {
    const groups = buildStaticGroups(markets);

    expect(groups.map((g) => g.heading)).toEqual([
      "Actions",
      "Pages",
      "Markets",
    ]);
  });

  it("links every market to its landing page", () => {
    const marketsGroup = buildStaticGroups(markets)[2];

    expect(marketsGroup.entries).toEqual([
      expect.objectContaining({
        label: "OSRS",
        target: { kind: "link", href: "/plutus/m1" },
      }),
    ]);
  });

  it("adds market-scoped pages and create actions", () => {
    const [actions, pages] = buildStaticGroups(markets);

    expect(pages.entries).toContainEqual(
      expect.objectContaining({
        label: "Strategies",
        hint: "OSRS",
        target: { kind: "link", href: "/plutus/m1/strategies" },
      }),
    );
    expect(actions.entries).toContainEqual(
      expect.objectContaining({
        label: "New strategy",
        hint: "OSRS",
        target: { kind: "link", href: "/plutus/m1/strategies/create" },
      }),
    );
  });

  it("includes the home page with a label and every theme action", () => {
    const [actions, pages] = buildStaticGroups([]);

    expect(pages.entries).toContainEqual(
      expect.objectContaining({
        label: "Home",
        target: { kind: "link", href: "/" },
      }),
    );
    expect(
      actions.entries
        .filter((e) => e.target.kind === "theme")
        .map((e) => e.target),
    ).toEqual([
      { kind: "theme", theme: "light" },
      { kind: "theme", theme: "dark" },
      { kind: "theme", theme: "system" },
    ]);
  });

  it("produces unique entry ids", () => {
    const ids = buildStaticGroups(markets).flatMap((g) =>
      g.entries.map((e) => e.id),
    );

    expect(new Set(ids).size).toBe(ids.length);
  });
});
