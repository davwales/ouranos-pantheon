import { describe, expect, it } from "vitest";
import { getSearchSuggestions } from "./search-suggestions";
import { type ExtendedColumnDef } from "./types";

const columns: ExtendedColumnDef<Record<string, unknown>>[] = [
  {
    id: "symbolName",
    header: "Name",
    filterConfig: { type: "string", operators: ["eq", "contains"], searchable: true },
  },
  {
    id: "maxPrice",
    header: "Max Price",
    filterConfig: { type: "number", operators: ["gt", "lt"] },
  },
  {
    id: "minPrice",
    header: "Min Price",
    filterConfig: { type: "number", operators: ["gt", "lt"] },
  },
  {
    id: "status",
    header: "Status",
    filterConfig: { type: "enum", operators: ["eq"], enumValues: ["Open", "Closed Early"] },
  },
  {
    id: "isActive",
    header: "Active",
    filterConfig: { type: "boolean", operators: ["eq"] },
  },
  { id: "notes", header: "Notes" },
];

function labels(query: string): string[] {
  return getSearchSuggestions(query, columns).map((s) => s.label);
}

describe("getSearchSuggestions", () => {
  it("returns nothing for empty or very short input", () => {
    expect(labels("")).toEqual([]);
    expect(labels("m")).toEqual([]);
  });

  it("suggests filterable fields matching the trailing word", () => {
    expect(getSearchSuggestions("gold ma", columns)).toEqual([
      { label: "Max Price", query: "gold Max Price " },
    ]);
  });

  it("matches multi-word field prefixes", () => {
    expect(labels("max pr")).toEqual(["Max Price"]);
  });

  it("ignores columns without filterConfig", () => {
    expect(labels("no")).toEqual([]);
  });

  it("does not suggest fields where a value is expected", () => {
    expect(labels("Name contains st")).toEqual([]);
  });

  it("suggests operators after a complete field", () => {
    expect(getSearchSuggestions("Max Price ", columns)).toEqual([
      { label: ">", query: "Max Price > " },
      { label: "<", query: "Max Price < " },
    ]);
  });

  it("suggests enum values after an operator, quoting multi-word values", () => {
    expect(getSearchSuggestions("Status is ", columns)).toEqual([
      { label: "Open", query: "Status is Open " },
      { label: "Closed Early", query: 'Status is "Closed Early" ' },
    ]);
  });

  it("suggests boolean values after an operator", () => {
    expect(labels("Active = ")).toEqual(["true", "false"]);
  });

  it("returns nothing after plain search text", () => {
    expect(labels("gold ")).toEqual([]);
  });
});
