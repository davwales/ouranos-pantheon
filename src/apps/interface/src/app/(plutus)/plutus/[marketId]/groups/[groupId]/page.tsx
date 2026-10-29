"use client";

import { type SelectedSymbol } from "@/app/(plutus)/plutus/components/symbol-search";
import TimeFrameSelection from "@/app/(plutus)/plutus/components/time-frame-selection";
import { useBreadcrumbLabel } from "@/components/shared/breadcrumbs";
import { NotFoundCard } from "@/components/shared/not-found-card";
import { useApi } from "@/hooks/use-api";
import { useUnsavedChangesGuard } from "@/hooks/use-unsaved-changes-guard";
import { type SymbolGroupSymbol, plutusApi } from "@/lib/api/plutus";
import { hasChanges } from "@/lib/has-changes";
import { type PlutusState, usePlutusStore } from "@/stores/plutus-store";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { useShallow } from "zustand/react/shallow";
import { AddGroupSymbolDialog } from "./_components/add-group-symbol-dialog";
import { GroupDetailHeader } from "./_components/group-detail-header";
import { GroupDetailSkeleton } from "./_components/group-detail-skeleton";
import { GroupDetailsForm } from "./_components/group-details-form";
import { GroupSymbolsTable } from "./_components/group-symbols-table";

export default function GroupDetailPage() {
  const { marketId, groupId } = useParams<{
    marketId: string;
    groupId: string;
  }>();
  const router = useRouter();

  const [timeFrameKey] = usePlutusStore(
    useShallow((state: PlutusState) => [state.timeFrameKey]),
  );

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [symbols, setSymbols] = useState<SymbolGroupSymbol[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [state, reexecute] = useApi(
    () => plutusApi.getSymbolGroup(groupId, timeFrameKey),
    [groupId, timeFrameKey],
  );

  const group = state.data;
  useBreadcrumbLabel(groupId, group?.name, state.status === "error");

  const editable = {
    name,
    description,
    symbolIds: symbols.map((s) => s.symbolId),
  };
  const [savedGroup, setSavedGroup] = useState<typeof editable>();
  useUnsavedChangesGuard(
    savedGroup !== undefined && hasChanges(editable, savedGroup),
  );

  const [syncedGroup, setSyncedGroup] = useState(group);
  if (group !== syncedGroup) {
    setSyncedGroup(group);
    if (group) {
      setName(group.name);
      setDescription(group.description ?? "");
      setSymbols(group.symbols);
      setSavedGroup({
        name: group.name,
        description: group.description ?? "",
        symbolIds: group.symbols.map((s) => s.symbolId),
      });
    }
  }

  const handleSave = async () => {
    setIsProcessing(true);
    try {
      await plutusApi.updateSymbolGroup({
        symbolGroupId: groupId,
        name,
        description: description || null,
        symbolIds: symbols.map((s) => s.symbolId),
      });
      reexecute();
    } catch (error) {
      console.error("Failed to save group:", error);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleDelete = async () => {
    setIsProcessing(true);
    try {
      await plutusApi.deleteSymbolGroup(groupId);
      router.push(`/plutus/${marketId}/groups`);
    } catch (error) {
      console.error("Failed to delete group:", error);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleAddSymbols = (selectedSymbols: SelectedSymbol[]) => {
    const existingIds = new Set(symbols.map((s) => s.symbolId));
    const newSymbols = selectedSymbols
      .filter((s) => !existingIds.has(s.id))
      .map((s) => ({
        symbolId: s.id,
        name: s.name,
        code: "",
        subcode: null,
        addedAt: new Date().toISOString(),
      }));
    if (newSymbols.length > 0) {
      setSymbols((prev) => [...prev, ...newSymbols]);
    }
  };

  const handleRemoveSymbol = (symbolId: string) => {
    setSymbols((prev) => prev.filter((s) => s.symbolId !== symbolId));
  };

  if (state.status === "error" && !group) {
    return <NotFoundCard title="Group not found" backHref={`/plutus/${marketId}/groups`} backLabel="Back to Groups" />;
  }

  if (state.status === "loading" && !group) {
    return <GroupDetailSkeleton />;
  }

  return (
    <div>
      <GroupDetailHeader
        isProcessing={isProcessing}
        canSave={!!name.trim()}
        onSave={handleSave}
        onDelete={handleDelete}
      />

      <GroupDetailsForm
        name={name}
        description={description}
        onNameChange={setName}
        onDescriptionChange={setDescription}
      />

      <div className="mt-8">
        <div className="flex items-center justify-between mb-1">
          <h3 className="text-lg font-medium">Symbols ({symbols.length})</h3>
          <div className="flex items-center gap-3">
            <div className="hidden sm:block">
              <TimeFrameSelection />
            </div>
            <AddGroupSymbolDialog
              marketId={marketId}
              excludeIds={symbols.map((s) => s.symbolId)}
              onAdd={handleAddSymbols}
            />
          </div>
        </div>
        <div className="sm:hidden mb-2 w-full">
          <TimeFrameSelection className="w-full" triggerClassName="w-full" />
        </div>

        <GroupSymbolsTable
          marketId={marketId}
          symbols={symbols}
          onRemoveSymbol={handleRemoveSymbol}
        />
      </div>
    </div>
  );
}
