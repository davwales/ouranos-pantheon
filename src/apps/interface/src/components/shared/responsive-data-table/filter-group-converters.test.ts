import { describe, expect, it } from "vitest";
import {
  filterGroupToQuery,
  queryToFilterGroup,
} from "./filter-group-converters";
import { FilterCondition, FilterGroup } from "./types";

const columns = [
  { id: "name", header: "Name" },
  { id: "type", header: "Type" },
  { id: "limit", header: "Limit" },
  { id: "isActive", header: "Active" },
  { id: "volume", header: "Volume" },
  { id: "margin", header: "Margin" },
  { id: "maxPrice", header: "Max Price" },
];

function cond(
  field: string,
  operator: FilterCondition["operator"],
  value: unknown,
): FilterCondition {
  return { field, operator, value };
}

function and(...items: FilterGroup["items"]): FilterGroup {
  return { logic: "and", items };
}

function or(...items: FilterGroup["items"]): FilterGroup {
  return { logic: "or", items };
}

describe("filterGroupToQuery", () => {
  it("returns empty string for empty group", () => {
    expect(filterGroupToQuery({ logic: "and", items: [] })).toBe("");
  });

  it("serializes a single condition", () => {
    expect(
      filterGroupToQuery(and(cond("name", "contains", "sword")), columns),
    ).toBe("Name contains sword");
  });

  it("serializes AND conditions", () => {
    expect(
      filterGroupToQuery(
        and(cond("name", "contains", "sword"), cond("limit", "gte", 100)),
        columns,
      ),
    ).toBe("Name contains sword and Limit ≥ 100");
  });

  it("serializes OR group with parentheses", () => {
    expect(
      filterGroupToQuery(
        and(
          cond("type", "eq", "weapon"),
          or(
            cond("name", "contains", "sword"),
            cond("name", "contains", "shield"),
          ),
        ),
        columns,
      ),
    ).toBe("Type is weapon and (Name contains sword or Name contains shield)");
  });

  it("serializes null values", () => {
    expect(filterGroupToQuery(and(cond("name", "eq", null)), columns)).toBe(
      "Name is null",
    );
    expect(filterGroupToQuery(and(cond("name", "neq", null)), columns)).toBe(
      "Name is not null",
    );
  });

  it("quotes string values that contain spaces", () => {
    expect(
      filterGroupToQuery(and(cond("name", "eq", "potion of healing")), columns),
    ).toBe('Name is "potion of healing"');
  });

  it("serializes comparison operators", () => {
    expect(filterGroupToQuery(and(cond("limit", "gt", 100)), columns)).toBe(
      "Limit > 100",
    );
    expect(filterGroupToQuery(and(cond("limit", "lt", 100)), columns)).toBe(
      "Limit < 100",
    );
    expect(filterGroupToQuery(and(cond("limit", "gte", 100)), columns)).toBe(
      "Limit ≥ 100",
    );
    expect(filterGroupToQuery(and(cond("limit", "lte", 100)), columns)).toBe(
      "Limit ≤ 100",
    );
    expect(filterGroupToQuery(and(cond("name", "startsWith", "a")), columns)).toBe(
      "Name starts with a",
    );
    expect(filterGroupToQuery(and(cond("name", "endsWith", "z")), columns)).toBe(
      "Name ends with z",
    );
  });

  it("serializes with human-readable header names", () => {
    expect(
      filterGroupToQuery(and(cond("maxPrice", "gt", 10000)), columns),
    ).toBe("Max Price > 10000");
  });
});

describe("queryToFilterGroup", () => {
  it("returns empty AND group for empty input", () => {
    expect(queryToFilterGroup("", columns)).toEqual({
      logic: "and",
      items: [],
    });
  });

  it("parses a simple comparison", () => {
    expect(queryToFilterGroup("name contains sword", columns)).toEqual(
      and(cond("name", "contains", "sword")),
    );
  });

  it("resolves multi-word header as field name", () => {
    expect(queryToFilterGroup("Max Price > 10000", columns)).toEqual(
      and(cond("maxPrice", "gt", "10000")),
    );
  });

  it("resolves field id as field name", () => {
    expect(queryToFilterGroup("maxPrice > 10000", columns)).toEqual(
      and(cond("maxPrice", "gt", "10000")),
    );
  });

  it("parses AND of two comparisons", () => {
    expect(
      queryToFilterGroup("name contains sword and limit >= 100", columns),
    ).toEqual(
      and(cond("name", "contains", "sword"), cond("limit", "gte", "100")),
    );
  });

  it("parses OR of two comparisons into nested OR group", () => {
    expect(
      queryToFilterGroup("name contains sword or limit >= 100", columns),
    ).toEqual(
      and(or(cond("name", "contains", "sword"), cond("limit", "gte", "100"))),
    );
  });

  it("parses nested parentheses", () => {
    expect(
      queryToFilterGroup(
        "(limit >= 100 or name contains potion) and isActive is true",
        columns,
      ),
    ).toEqual(
      and(
        or(cond("limit", "gte", "100"), cond("name", "contains", "potion")),
        cond("isActive", "eq", "true"),
      ),
    );
  });

  it("handles ((A and B) or C) nested pattern", () => {
    const result = queryToFilterGroup(
      "((Name contains soulreaper and Volume > 1000000) or Margin > 100)",
      columns,
    );
    expect(result).toEqual(
      and(
        or(
          and(
            cond("name", "contains", "soulreaper"),
            cond("volume", "gt", "1000000"),
          ),
          cond("margin", "gt", "100"),
        ),
      ),
    );
  });

  it("returns null for invalid input", () => {
    expect(queryToFilterGroup("xyz >= 100", columns)).toBeNull();
    expect(queryToFilterGroup("limit >=", columns)).toBeNull();
    expect(queryToFilterGroup("(limit >= 100", columns)).toBeNull();
    expect(queryToFilterGroup('name is "potion', columns)).toBeNull();
  });
});

describe("round-trip", () => {
  it("serializes and parses back a simple condition", () => {
    const tree = and(cond("name", "contains", "sword"));
    const parsed = queryToFilterGroup(filterGroupToQuery(tree, columns), columns);
    expect(parsed).toEqual(tree);
  });

  it("serializes and parses back a nested group", () => {
    const tree = and(
      cond("type", "eq", "weapon"),
      or(cond("name", "contains", "sword"), cond("limit", "gte", 100)),
    );
    const parsed = queryToFilterGroup(filterGroupToQuery(tree, columns), columns);
    expect(parsed).toEqual(
      and(
        cond("type", "eq", "weapon"),
        or(cond("name", "contains", "sword"), cond("limit", "gte", "100")),
      ),
    );
  });

  it("round-trips a condition with a multi-word header column", () => {
    const tree = and(cond("maxPrice", "gt", 10000));
    const serialized = filterGroupToQuery(tree, columns);
    const parsed = queryToFilterGroup(serialized, columns);
    expect(serialized).toBe("Max Price > 10000");
    expect(parsed).toEqual(and(cond("maxPrice", "gt", "10000")));
  });

  it("parses (A or B) and C with multi-word header", () => {
    expect(
      queryToFilterGroup(
        "(name contains sword or name contains axe) and maxPrice > 10000",
        columns,
      ),
    ).toEqual(
      and(
        or(cond("name", "contains", "sword"), cond("name", "contains", "axe")),
        cond("maxPrice", "gt", "10000"),
      ),
    );
  });
});
