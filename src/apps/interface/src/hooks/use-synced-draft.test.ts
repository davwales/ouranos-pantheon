import { act, renderHook } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { useSyncedDraft, withRefreshedRows } from "./use-synced-draft";

type Row = { symbolId: string; quantity: number; price: number };
type Entity = { id: string; name: string; rows: Row[] };
type Draft = { name: string; rows: Row[] };

const toDraft = (entity: Entity): Draft => ({
  name: entity.name,
  rows: entity.rows,
});
const refresh = (draft: Draft, entity: Entity): Draft => ({
  ...draft,
  rows: withRefreshedRows(draft.rows, entity.rows, (row) => ({
    quantity: row.quantity,
  })),
});

function renderDraft(initial: Entity) {
  return renderHook(
    ({ data }) =>
      useSyncedDraft(data, toDraft, refresh, { name: "", rows: [] }),
    { initialProps: { data: initial as Entity | undefined } },
  );
}

const entity: Entity = {
  id: "a",
  name: "Saved",
  rows: [{ symbolId: "s1", quantity: 1, price: 10 }],
};
const refetched: Entity = {
  ...entity,
  rows: [{ symbolId: "s1", quantity: 1, price: 20 }],
};

describe("useSyncedDraft", () => {
  it("seeds the draft from fetched data and starts clean", () => {
    // Arrange / Act
    const { result } = renderDraft(entity);

    // Assert
    expect(result.current.draft).toEqual(toDraft(entity));
    expect(result.current.isDirty).toBe(false);
  });

  it("keeps unsaved edits on a refetch and refreshes server fields", () => {
    // Arrange
    const { result, rerender } = renderDraft(entity);
    act(() =>
      result.current.setDraft((prev) => ({
        name: "Edited",
        rows: [{ ...prev.rows[0], quantity: 5 }],
      })),
    );

    // Act
    rerender({ data: refetched });

    // Assert
    expect(result.current.draft).toEqual({
      name: "Edited",
      rows: [{ symbolId: "s1", quantity: 5, price: 20 }],
    });
    expect(result.current.isDirty).toBe(true);
  });

  it("reseeds a clean draft on a refetch", () => {
    // Arrange
    const { result, rerender } = renderDraft(entity);

    // Act
    rerender({ data: refetched });

    // Assert
    expect(result.current.draft).toEqual(toDraft(refetched));
    expect(result.current.isDirty).toBe(false);
  });

  it("reseeds a dirty draft when a different entity loads", () => {
    // Arrange
    const { result, rerender } = renderDraft(entity);
    act(() => result.current.setDraft((prev) => ({ ...prev, name: "Edited" })));
    const other: Entity = { id: "b", name: "Other", rows: [] };

    // Act
    rerender({ data: other });

    // Assert
    expect(result.current.draft).toEqual(toDraft(other));
    expect(result.current.isDirty).toBe(false);
  });

  it("is clean once saved", () => {
    // Arrange
    const { result } = renderDraft(entity);
    act(() => result.current.setDraft((prev) => ({ ...prev, name: "Edited" })));

    // Act
    act(() => result.current.markSaved());

    // Assert
    expect(result.current.isDirty).toBe(false);
  });
});

describe("withRefreshedRows", () => {
  it("keeps draft membership and order, refreshing matching rows", () => {
    // Arrange
    const rows: Row[] = [
      { symbolId: "new", quantity: 1, price: 0 },
      { symbolId: "s1", quantity: 3, price: 10 },
    ];
    const fetched: Row[] = [
      { symbolId: "s1", quantity: 1, price: 20 },
      { symbolId: "removed", quantity: 1, price: 30 },
    ];

    // Act
    const refreshed = withRefreshedRows(rows, fetched);

    // Assert
    expect(refreshed).toEqual([
      { symbolId: "new", quantity: 1, price: 0 },
      { symbolId: "s1", quantity: 1, price: 20 },
    ]);
  });
});
