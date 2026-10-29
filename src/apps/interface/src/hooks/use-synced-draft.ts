"use client";

import { hasChanges } from "@/lib/has-changes";
import { useState } from "react";
import { useUnsavedChangesGuard } from "./use-unsaved-changes-guard";

/**
 * Editor state for an entity that is refetched while the editor stays open (e.g. on a time-frame
 * change). The draft is reseeded from fetched data only while it has no unsaved edits or when a
 * different entity loads; otherwise `refresh` merges the refetched data into the draft so the
 * edits survive. The baseline is the last saved values and guards unload while dirty.
 */
export function useSyncedDraft<TData extends { id: string }, TDraft>(
  data: TData | undefined,
  toDraft: (data: TData) => TDraft,
  refresh: (draft: TDraft, data: TData) => TDraft,
  initialDraft: TDraft,
) {
  const [draft, setDraft] = useState(initialDraft);
  const [baseline, setBaseline] = useState<TDraft>();
  const [syncedData, setSyncedData] = useState<TData>();

  const isDirty = baseline !== undefined && hasChanges(draft, baseline);
  useUnsavedChangesGuard(isDirty);

  if (data && data !== syncedData) {
    setSyncedData(data);
    const fetched = toDraft(data);
    setDraft(
      isDirty && syncedData?.id === data.id ? refresh(draft, data) : fetched,
    );
    setBaseline(fetched);
  }

  return {
    draft,
    setDraft,
    isDirty,
    markSaved: () => setBaseline(draft),
  };
}

/**
 * Refreshes the server-computed fields of draft rows from refetched rows, keeping the draft's
 * row membership and order and the fields chosen by `keep`.
 */
export function withRefreshedRows<TRow extends { symbolId: string }>(
  rows: TRow[],
  fetchedRows: TRow[],
  keep: (row: TRow) => Partial<TRow> = () => ({}),
): TRow[] {
  const fetchedById = new Map(fetchedRows.map((row) => [row.symbolId, row]));
  return rows.map((row) => {
    const fetched = fetchedById.get(row.symbolId);
    return fetched ? { ...fetched, ...keep(row) } : row;
  });
}
