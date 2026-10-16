import {
  applyExplorerView,
  explorerViewHandler,
} from "@/app/(plutus)/plutus/[marketId]/explorer/_components/explorer-assistant-actions";
import { explorerColumns } from "@/app/(plutus)/plutus/[marketId]/explorer/_components/explorer-columns";
import { serializeFilterGroup } from "@/components/shared/responsive-data-table/types";
import { type SetExplorerViewAction } from "@/lib/api/plutus-types";
import { describe, expect, it, vi } from "vitest";

const COLUMNS = explorerColumns("market-1");

const ACTION: SetExplorerViewAction = {
  conditions: [
    { field: "roi", operator: "gt", value: "0.05" },
    { field: "symbolName", operator: "like", value: "rune" },
  ],
  sortField: "margin",
  descending: false,
};

describe("applyExplorerView", () => {
  it("WhenActionArrives_ShouldReplaceTheFilterSortAndResetToTheFirstPage", () => {
    // Arrange
    const state = {
      pagination: { pageSize: 10, skip: 30, take: 10 },
      sort: { totalGain: "DESC" as const },
    };

    // Act
    const next = applyExplorerView(state, ACTION, COLUMNS);

    // Assert
    expect(serializeFilterGroup(next.filter ?? { logic: "and", items: [] })).toEqual([
      "roi:gt:0.05",
      "symbolName:like:rune",
    ]);
    expect(next.sort).toEqual({ margin: "ASC" });
    expect(next.pagination).toEqual({ pageSize: 10, skip: 0, take: 10 });
    expect(next.searchQuery).toBeTruthy();
  });

  it("WhenActionHasNoSort_ShouldKeepTheCurrentSort", () => {
    // Arrange
    const state = { sort: { totalGain: "DESC" as const } };

    // Act
    const next = applyExplorerView(
      state,
      { ...ACTION, sortField: null },
      COLUMNS,
    );

    // Assert
    expect(next.sort).toEqual({ totalGain: "DESC" });
    expect(next.filter?.items).toHaveLength(2);
  });
});

describe("explorerViewHandler", () => {
  it("ShouldWaitForTheUserToApplyAndDescribeTheView", () => {
    // Arrange
    const apply = vi.fn();

    // Act
    const handler = explorerViewHandler(COLUMNS, apply);
    handler.apply(ACTION);

    // Assert
    expect(handler.autoApply).toBeFalsy();
    expect(apply).toHaveBeenCalledWith(ACTION);
    expect(handler.describe(ACTION)).toMatch(/^Showing .*ROI.*, sorted by Margin ascending$/);
  });
});
