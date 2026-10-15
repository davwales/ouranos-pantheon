import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { useState } from "react";
import { describe, expect, it, vi } from "vitest";
import { DataTableFiltering } from "./data-table-filtering";
import { type DataTableState, type ExtendedColumnDef } from "./types";

const columns: ExtendedColumnDef<unknown>[] = [
  {
    id: "symbolName",
    header: "Name",
    filterConfig: { type: "string", operators: ["contains"], searchable: true },
  },
  {
    id: "volume",
    header: "Volume",
    filterConfig: { type: "number", operators: ["gt", "lt"] },
  },
];

function Harness({
  initial,
  onState,
}: {
  initial?: DataTableState;
  onState: (state: DataTableState) => void;
}) {
  const [state, setState] = useState<DataTableState | undefined>(initial);
  return (
    <DataTableFiltering
      columns={columns}
      state={state}
      onStateChange={(next) => {
        setState(next);
        onState(next);
      }}
    />
  );
}

describe("DataTableFiltering", () => {
  it("shows the applied filter as search text when no query was typed", () => {
    render(
      <Harness
        initial={{ filter: { logic: "and", items: [{ field: "volume", operator: "gt", value: 1 }] } }}
        onState={vi.fn()}
      />,
    );

    expect(screen.getByLabelText("Search and filter")).toHaveValue("Volume > 1");
    expect(screen.getByText("Volume > 1", { selector: "[data-slot=badge]" })).toBeInTheDocument();
  });

  it("applies typed free text and conditions after debounce", async () => {
    const onState = vi.fn();
    render(<Harness onState={onState} />);

    fireEvent.change(screen.getByLabelText("Search and filter"), {
      target: { value: "gold volume > 10" },
    });

    await waitFor(() =>
      expect(onState).toHaveBeenLastCalledWith(
        expect.objectContaining({
          searchQuery: "gold volume > 10",
          filter: {
            logic: "and",
            items: [
              {
                logic: "or",
                search: "gold",
                items: [{ field: "symbolName", operator: "contains", value: "gold" }],
              },
              { field: "volume", operator: "gt", value: "10" },
            ],
          },
        }),
      ),
    );
    expect(screen.getByText("“gold”")).toBeInTheDocument();
  });

  it("removes a chip and rewrites the search text", () => {
    const onState = vi.fn();
    render(
      <Harness
        initial={{
          searchQuery: "gold volume > 10",
          filter: {
            logic: "and",
            items: [
              {
                logic: "or",
                search: "gold",
                items: [{ field: "symbolName", operator: "contains", value: "gold" }],
              },
              { field: "volume", operator: "gt", value: "10" },
            ],
          },
        }}
        onState={onState}
      />,
    );

    fireEvent.click(screen.getByLabelText("Remove filter “gold”"));

    expect(onState).toHaveBeenLastCalledWith(
      expect.objectContaining({
        searchQuery: "Volume > 10",
        filter: { logic: "and", items: [{ field: "volume", operator: "gt", value: "10" }] },
      }),
    );
    expect(screen.getByLabelText("Search and filter")).toHaveValue("Volume > 10");
  });

  it("clears search text and filter", () => {
    const onState = vi.fn();
    render(<Harness initial={{ searchQuery: "gold" }} onState={onState} />);

    fireEvent.click(screen.getByLabelText("Clear search"));

    expect(onState).toHaveBeenLastCalledWith(
      expect.objectContaining({ searchQuery: "", filter: { logic: "and", items: [] } }),
    );
  });

  it("accepts the first suggestion with Tab", () => {
    render(<Harness onState={vi.fn()} />);
    const input = screen.getByLabelText("Search and filter");

    fireEvent.change(input, { target: { value: "vol" } });
    fireEvent.keyDown(input, { key: "Tab" });

    expect(input).toHaveValue("Volume ");
  });
});
