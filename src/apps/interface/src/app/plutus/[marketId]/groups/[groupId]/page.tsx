"use client";

import { ConfirmationButton } from "@/app/components/confirmation-button";
import { PrettyNumber } from "@/app/components/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import ResponsiveDataTable from "@/app/components/responsive-data-table/responsive-data-table";
import { ResponsiveDialog } from "@/app/components/responsive-dialog/responsive-dialog";
import { ResponsiveDropdownMenu } from "@/app/components/responsive-dropdown-menu/responsive-dropdown-menu";
import { Typography } from "@/app/components/typography";
import {
  SelectedSymbol,
  SymbolSearch,
} from "@/app/plutus/components/symbol-search";
import TimeFrameSelection from "@/app/plutus/components/time_frame_selection";
import { PlutusState, usePlutusStore } from "@/app/plutus/plutus_store";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useApi } from "@/hooks/use-api";
import { SymbolGroupSymbol, plutusApi } from "@/lib/api/plutus";
import { MoreHorizontal, Plus, RefreshCw, Trash2 } from "lucide-react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { useShallow } from "zustand/react/shallow";

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
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [selectedSymbols, setSelectedSymbols] = useState<SelectedSymbol[]>([]);
  const [state, reexecute] = useApi(
    () => plutusApi.getSymbolGroup(groupId, timeFrameKey),
    [groupId, timeFrameKey],
  );

  const group = state.data;
  const fetching = state.status === "loading";

  useEffect(() => {
    if (group) {
      setName(group.name);
      setDescription(group.description ?? "");
      setSymbols(group.symbols);
    }
  }, [group]);

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

  const handleAddSymbol = () => {
    if (selectedSymbols.length === 0) return;
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
    setIsAddDialogOpen(false);
    setSelectedSymbols([]);
  };

  const handleRemoveSymbol = (symbolId: string) => {
    setSymbols((prev) => prev.filter((s) => s.symbolId !== symbolId));
  };

  const columns: ExtendedColumnDef<SymbolGroupSymbol>[] = useMemo(
    () => [
      {
        id: "name",
        header: "Symbol",
        accessorFn: (row) => row.name,
        cell: ({ row, getValue }) => (
          <Link
            href={`/plutus/${marketId}/${row.original.symbolId}`}
            className="hover:underline"
          >
            {getValue<string>()}
            {row.original.subcode && (
              <span className="text-muted-foreground ml-2 text-sm">
                {row.original.subcode}
              </span>
            )}
          </Link>
        ),
      },
      {
        id: "volume",
        header: "Volume",
        accessorFn: (row) => row.volume,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? <PrettyNumber number={val} /> : "-";
        },
      },
      {
        id: "gain",
        header: "Gain",
        accessorFn: (row) => row.gain,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? <PrettyNumber number={val} /> : "-";
        },
      },
      {
        id: "roi",
        header: "ROI",
        accessorFn: (row) => row.roi,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? `${(val * 100).toFixed(2)}%` : "-";
        },
      },
      {
        id: "signalScore",
        header: "Signal",
        accessorFn: (row) => row.signalScore,
        cell: ({ getValue }) => {
          const val = getValue<number | null>();
          return val != null ? val.toFixed(3) : "-";
        },
      },
      {
        id: "actions",
        header: "",
        cell: ({ row }) => (
          <ResponsiveDropdownMenu
            title="Symbol Actions"
            description="Available actions for this symbol"
            actions={[
              {
                label: "Remove",
                icon: <Trash2 className="h-4 w-4 text-destructive" />,
                onClick: () => handleRemoveSymbol(row.original.symbolId),
              },
            ]}
          >
            <div className="border rounded-md">
              <Button variant="ghost" className="w-full">
                <MoreHorizontal className="m-auto" />
              </Button>
            </div>
          </ResponsiveDropdownMenu>
        ),
      },
    ],
    [marketId],
  );

  if (fetching) {
    return <div>Loading...</div>;
  }

  if (!group) {
    return <div>Group not found</div>;
  }

  return (
    <div>
      <div className="flex items-center justify-between">
        <Typography variant="h2" className="border-b-0">
          Group Details
        </Typography>
        <div className="flex items-center gap-2">
          {isProcessing && <RefreshCw className="animate-spin" />}
          <Button onClick={handleSave} disabled={isProcessing || !name.trim()}>
            Save
          </Button>
          <ConfirmationButton
            title="Delete Group"
            description="Are you sure you want to delete this group? This action cannot be undone."
            onConfirm={handleDelete}
            disabled={isProcessing}
            variant="destructive"
          >
            Delete
          </ConfirmationButton>
        </div>
      </div>

      <div className="mt-4 space-y-4 max-w-lg">
        <div className="space-y-1">
          <label htmlFor="name" className="text-sm font-medium">
            Name
          </label>
          <Input
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="space-y-1">
          <label htmlFor="description" className="text-sm font-medium">
            Description (optional)
          </label>
          <Textarea
            id="description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
          />
        </div>
      </div>

      <div className="mt-8">
        <div className="flex items-center justify-between mb-1">
          <h3 className="text-lg font-medium">Symbols ({symbols.length})</h3>
          <div className="flex items-center gap-3">
            <div className="hidden sm:block">
              <TimeFrameSelection />
            </div>
            <ResponsiveDialog
              title="Add Symbol"
              description="Search for a symbol to add to this group"
              open={isAddDialogOpen}
              onOpenChange={setIsAddDialogOpen}
              trigger={
                <Button variant="ghost">
                  <Plus className="w-4 h-4 mr-1" />
                  Add Symbol
                </Button>
              }
            >
              <div>
                <SymbolSearch
                  marketId={marketId}
                  excludeIds={symbols.map((s) => s.symbolId)}
                  onSymbolsChanged={setSelectedSymbols}
                />
                <Button
                  onClick={handleAddSymbol}
                  disabled={selectedSymbols.length === 0}
                  className="mt-2 w-full"
                >
                  {selectedSymbols.length > 0
                    ? `Add (${selectedSymbols.length})`
                    : "Add"}
                </Button>
              </div>
            </ResponsiveDialog>
          </div>
        </div>
        <div className="sm:hidden mb-2 w-full">
          <TimeFrameSelection className="w-full" triggerClassName="w-full" />
        </div>

        <ResponsiveDataTable
          columns={columns}
          data={symbols}
          scrollTop={false}
          disablePagination
          disableFiltering
          disableSorting
          className="my-2"
        />
      </div>
    </div>
  );
}
