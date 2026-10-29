import { type DataTableState } from "@/components/shared/responsive-data-table/types";
import { type PagedResponse } from "@/lib/api-client";
import { type Position, plutusApi } from "@/lib/api/plutus";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { SymbolPositionsView } from "./symbol-positions-view";

vi.mock("@/lib/api/plutus", () => ({
  plutusApi: { getAllPositions: vi.fn() },
}));

vi.mock(
  "@/components/shared/responsive-data-table/responsive-data-table",
  () => ({
    default: ({
      loading,
      data,
      state,
      onStateChange,
      pageInfo,
    }: {
      loading?: boolean;
      data?: Position[];
      state?: DataTableState;
      onStateChange?: (state: DataTableState) => void;
      pageInfo?: { totalCount?: number };
    }) =>
      loading ? (
        <div>Loading table</div>
      ) : (
        <div>
          <span>{`${data?.length ?? 0} of ${pageInfo?.totalCount ?? 0}`}</span>
          <button
            onClick={() =>
              onStateChange?.({
                ...state,
                pagination: { ...state?.pagination, skip: 10, take: 10 },
              })
            }
          >
            Next page
          </button>
        </div>
      ),
  }),
);

vi.mock(
  "@/app/(plutus)/plutus/[marketId]/_components/positions/create-position-dialog",
  () => ({ CreatePositionDialog: () => null }),
);

function page(totalCount: number, count = totalCount): PagedResponse<Position> {
  return {
    items: Array.from(
      { length: count },
      (_, i) => ({ id: `p${i}` }) as Position,
    ),
    totalCount,
    skip: 0,
    take: 10,
  };
}

function renderView() {
  return render(
    <SymbolPositionsView
      marketId="market-1"
      symbolId="symbol-1"
      symbolName="Symbol"
    />,
  );
}

describe("SymbolPositionsView", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("WhenLoading_ShouldShowLoadingTablesInsteadOfNoPositions", () => {
    // Arrange
    vi.mocked(plutusApi.getAllPositions).mockReturnValue(new Promise(() => {}));

    // Act
    renderView();

    // Assert
    expect(screen.getAllByText("Loading table")).toHaveLength(2);
    expect(screen.queryByText("No positions for this symbol.")).toBeNull();
  });

  it("WhenBothEmpty_ShouldShowNoPositions", async () => {
    // Arrange
    vi.mocked(plutusApi.getAllPositions).mockResolvedValue(page(0));

    // Act
    renderView();

    // Assert
    expect(
      await screen.findByText("No positions for this symbol."),
    ).toBeTruthy();
  });

  it("WhenMorePositionsThanOnePage_ShouldPaginate", async () => {
    // Arrange
    vi.mocked(plutusApi.getAllPositions).mockImplementation(
      async (_marketId, args) =>
        args?.filter?.[0]?.includes("status:eq:Pending")
          ? page(60, 10)
          : page(0),
    );
    renderView();
    await screen.findByText("10 of 60");

    // Act
    fireEvent.click(screen.getByText("Next page"));

    // Assert
    await waitFor(() =>
      expect(plutusApi.getAllPositions).toHaveBeenCalledWith(
        "market-1",
        expect.objectContaining({
          skip: 10,
          take: 10,
          filter: ["and(symbolId:eq:symbol-1|status:eq:Pending)"],
        }),
      ),
    );
  });
});
