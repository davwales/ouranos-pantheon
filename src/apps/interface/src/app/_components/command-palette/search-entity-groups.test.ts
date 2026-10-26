import { beforeEach, describe, expect, it, vi } from "vitest";
import { type Market } from "@/lib/api/plutus";
import { searchEntityGroups } from "./search-entity-groups";

const plutusApi = vi.hoisted(() => ({
  getAllSymbols: vi.fn(),
  getAllSymbolGroups: vi.fn(),
  getAllStrategies: vi.fn(),
  getAllRecipes: vi.fn(),
}));
const hestiaApi = vi.hoisted(() => ({ getAllRecipes: vi.fn() }));
const hermesApi = vi.hoisted(() => ({ getAllConversations: vi.fn() }));

vi.mock("@/lib/api/plutus", () => ({ plutusApi }));
vi.mock("@/lib/api/hestia", () => ({ hestiaApi }));
vi.mock("@/lib/api/hermes", () => ({ hermesApi }));

const markets = [{ id: "m1", name: "OSRS" } as Market];
const page = <T,>(items: T[]) => ({ items, totalCount: items.length, skip: 0, take: 5 });

describe("searchEntityGroups", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    plutusApi.getAllSymbols.mockResolvedValue(
      page([{ id: "s1", marketId: "m1", name: "Dragon bolts", subcode: "p2p" }]),
    );
    plutusApi.getAllSymbolGroups.mockResolvedValue(
      page([{ id: "g1", marketId: "m1", name: "Dragon gear" }]),
    );
    plutusApi.getAllStrategies.mockResolvedValue(
      page([{ id: "st1", marketId: "m1", name: "Dragon swing" }]),
    );
    plutusApi.getAllRecipes.mockResolvedValue(
      page([{ id: "r1", marketId: "m1", name: "Dragon bolts (e)" }]),
    );
    hestiaApi.getAllRecipes.mockResolvedValue(
      page([{ id: "k1", title: "Dragon fruit salad" }]),
    );
    hermesApi.getAllConversations.mockResolvedValue([
      { id: "c1", name: "Dragon lore" },
    ]);
  });

  it("queries every source with a name filter", async () => {
    await searchEntityGroups("drag", markets);

    expect(plutusApi.getAllSymbols).toHaveBeenCalledWith({
      filter: ["name:like:drag"],
      take: 5,
    });
    expect(plutusApi.getAllSymbolGroups).toHaveBeenCalledWith("m1", "OneDay", {
      filter: ["name:like:drag"],
      take: 5,
    });
    expect(plutusApi.getAllStrategies).toHaveBeenCalledWith("m1", {
      filter: ["name:like:drag"],
      take: 5,
    });
    expect(hestiaApi.getAllRecipes).toHaveBeenCalledWith({
      filter: ["title:like:drag"],
      take: 5,
    });
    expect(hermesApi.getAllConversations).toHaveBeenCalledWith({
      filter: ["name:like:drag"],
      sortField: "UpdatedAt",
      sortDirection: "desc",
    });
  });

  it("maps results to links on their detail pages", async () => {
    const groups = await searchEntityGroups("drag", markets);

    const hrefs = Object.fromEntries(
      groups.map((g) => [
        g.heading,
        g.entries.map((e) => (e.target.kind === "link" ? e.target.href : "")),
      ]),
    );
    expect(hrefs).toEqual({
      Symbols: ["/plutus/m1/s1"],
      "Symbol Groups": ["/plutus/m1/groups/g1"],
      Strategies: ["/plutus/m1/strategies/st1"],
      "Market Recipes": ["/plutus/m1/recipes/r1"],
      "Kitchen Recipes": ["/hestia/recipes/k1"],
      Conversations: ["/hermes/conversations/c1"],
    });
    expect(groups[0].entries[0]).toMatchObject({
      label: "Dragon bolts (p2p)",
      hint: "OSRS",
    });
  });

  it("drops a failing source instead of failing the search", async () => {
    plutusApi.getAllSymbols.mockRejectedValue(new Error("boom"));

    const groups = await searchEntityGroups("drag", markets);

    expect(groups.map((g) => g.heading)).not.toContain("Symbols");
    expect(groups).toHaveLength(5);
  });
});
