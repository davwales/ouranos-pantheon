import { describe, expect, it } from "vitest";
import { Zap } from "lucide-react";
import { type PaletteGroup } from "./command-palette-types";
import { filterGroups } from "./filter-groups";

const groups: PaletteGroup[] = [
  {
    heading: "Pages",
    icon: Zap,
    entries: [
      {
        id: "a",
        label: "Strategies",
        hint: "OSRS",
        target: { kind: "link", href: "/a" },
      },
      {
        id: "b",
        label: "Explorer",
        hint: "FFXIV",
        keywords: ["pricing trends"],
        target: { kind: "link", href: "/b" },
      },
    ],
  },
  { heading: "Empty", icon: Zap, entries: [] },
];

describe("filterGroups", () => {
  it("returns every non-empty group for a blank query", () => {
    const result = filterGroups(groups, "   ");

    expect(result.map((g) => g.heading)).toEqual(["Pages"]);
    expect(result[0].entries).toHaveLength(2);
  });

  it("matches label, hint and keywords case-insensitively", () => {
    expect(filterGroups(groups, "STRAT")[0].entries.map((e) => e.id)).toEqual([
      "a",
    ]);
    expect(filterGroups(groups, "ffxiv")[0].entries.map((e) => e.id)).toEqual([
      "b",
    ]);
    expect(filterGroups(groups, "trends")[0].entries.map((e) => e.id)).toEqual([
      "b",
    ]);
  });

  it("requires every token to match", () => {
    expect(filterGroups(groups, "strategies osrs")[0].entries).toHaveLength(1);
    expect(filterGroups(groups, "strategies ffxiv")).toEqual([]);
  });
});
