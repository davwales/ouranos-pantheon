import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import { ApiError } from "@/lib/api-client";
import { hestiaApi } from "@/lib/api/hestia";
import type {
  RecipeHistoryEvent,
  RecipeHistoryResponse,
} from "@/lib/api/hestia-types";
import { VersionHistoryDialog } from "./version-history-dialog";

vi.mock("@/lib/api/hestia", () => ({
  hestiaApi: {
    getRecipeHistory: vi.fn(),
    revertRecipe: vi.fn(),
  },
}));

vi.mock("@/components/shared/responsive-dialog/responsive-dialog", () => ({
  ResponsiveDialog: ({
    children,
    open,
  }: {
    title: string;
    description: string;
    trigger: React.ReactNode;
    children: React.ReactNode;
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
  }) => (
    <div data-testid="responsive-dialog" data-open={open ? "true" : "false"}>
      {open ? <div data-testid="dialog-content">{children}</div> : null}
    </div>
  ),
}));

function mockHistoryEvent(
  overrides: Partial<RecipeHistoryEvent> = {},
): RecipeHistoryEvent {
  return {
    version: 1,
    eventType: "recipe_created",
    timestamp: "2025-01-01T00:00:00.000Z",
    ...overrides,
  };
}

function mockHistoryResponse(
  overrides: Partial<RecipeHistoryResponse> = {},
): RecipeHistoryResponse {
  return {
    recipeId: "test-recipe-1",
    events: [
      mockHistoryEvent(),
      mockHistoryEvent({
        version: 2,
        eventType: "recipe_title_changed",
        timestamp: "2025-01-02T00:00:00.000Z",
      }),
    ],
    ...overrides,
  };
}

function rowFor(versionText: string): HTMLElement {
  return screen.getByText(versionText).closest("li")!;
}

describe("VersionHistoryDialog", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  function renderDialog(onReverted = vi.fn()) {
    render(
      <VersionHistoryDialog
        recipeId="test-recipe-1"
        open={true}
        onOpenChange={vi.fn()}
        onReverted={onReverted}
      />,
    );
    return onReverted;
  }

  it("renders versions newest first with labels, versions, and the current badge", async () => {
    vi.mocked(hestiaApi.getRecipeHistory).mockResolvedValue(mockHistoryResponse());

    renderDialog();

    await screen.findByText("Title Changed");

    const rows = screen.getAllByRole("listitem");
    expect(rows).toHaveLength(2);

    expect(within(rows[0]).getByText("Title Changed")).toBeInTheDocument();
    expect(within(rows[0]).getByText("v2")).toBeInTheDocument();
    expect(within(rows[0]).getByText("Current")).toBeInTheDocument();

    expect(within(rows[1]).getByText("Created")).toBeInTheDocument();
    expect(within(rows[1]).getByText("v1")).toBeInTheDocument();
  });

  it("shows an empty state when there are no events", async () => {
    vi.mocked(hestiaApi.getRecipeHistory).mockResolvedValue({
      recipeId: "test-recipe-1",
      events: [],
    });

    renderDialog();

    expect(
      await screen.findByText("No version history yet."),
    ).toBeInTheDocument();
  });

  it("shows an error banner when the history fetch fails", async () => {
    vi.mocked(hestiaApi.getRecipeHistory).mockRejectedValue(
      new ApiError(500, "Server error"),
    );

    renderDialog();

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Failed to load version history.",
    );
  });

  it("reverts to the selected version after confirming", async () => {
    vi.mocked(hestiaApi.getRecipeHistory).mockResolvedValue(mockHistoryResponse());
    vi.mocked(hestiaApi.revertRecipe).mockResolvedValue({ id: "test-recipe-1" });
    const onReverted = renderDialog();

    await screen.findByText("Title Changed");

    const oldestRow = rowFor("v1");
    fireEvent.click(within(oldestRow).getByRole("button", { name: /^revert$/i }));

    expect(
      within(oldestRow).getByText(/restore the recipe to v1/i),
    ).toBeInTheDocument();

    fireEvent.click(within(oldestRow).getByRole("button", { name: /^revert$/i }));

    await waitFor(() => {
      expect(hestiaApi.revertRecipe).toHaveBeenCalledWith("test-recipe-1", {
        targetVersion: 1,
      });
    });
    expect(onReverted).toHaveBeenCalledTimes(1);
    await waitFor(() => {
      expect(hestiaApi.getRecipeHistory).toHaveBeenCalledTimes(2);
    });
  });

  it("shows an error banner and does not notify the page when revert fails", async () => {
    vi.mocked(hestiaApi.getRecipeHistory).mockResolvedValue(mockHistoryResponse());
    vi.mocked(hestiaApi.revertRecipe).mockRejectedValue(
      new ApiError(400, "Revert failed."),
    );
    const onReverted = renderDialog();

    await screen.findByText("Title Changed");

    const oldestRow = rowFor("v1");
    fireEvent.click(within(oldestRow).getByRole("button", { name: /^revert$/i }));
    fireEvent.click(within(oldestRow).getByRole("button", { name: /^revert$/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent("Revert failed.");
    });
    expect(onReverted).not.toHaveBeenCalled();
  });

  it("marks the latest version as current and only offers revert on older versions", async () => {
    vi.mocked(hestiaApi.getRecipeHistory).mockResolvedValue(mockHistoryResponse());

    renderDialog();

    await screen.findByText("Title Changed");

    expect(
      screen.getAllByRole("button", { name: /^revert$/i }),
    ).toHaveLength(1);
    expect(rowFor("v2")).toContainElement(screen.getByText("Current"));
    expect(rowFor("v1")).not.toContainElement(screen.getByText("Current"));
  });
});
