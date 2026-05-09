import { PrettyNumber } from "@/app/components/pretty-number/pretty-number";
import { ExtendedColumnDef } from "@/app/components/responsive-data-table";
import Link from "next/link";

import { type Position } from "@/lib/api/plutus";
import {
  PositionActions,
  type PositionActionsVariant,
} from "./position-actions";
import {
  positionSideColors,
  positionStatusColors,
  positionStatusLabels,
} from "./position-constants";

function PositionStatusBadge({ status }: { status: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${positionStatusColors[status] ?? "bg-gray-100 text-gray-800"}`}
    >
      {positionStatusLabels[status] ?? status}
    </span>
  );
}

function LinkedBadge() {
  return (
    <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-blue-100 text-blue-800">
      Linked
    </span>
  );
}

export type PositionActionsConfig = {
  variant: PositionActionsVariant;
  onEdit?: (position: Position) => void;
  onClose?: (position: Position) => void;
  onLink?: (position: Position) => void;
  onCreateSell?: (position: Position) => void;
};

export type MakePositionColumnsOptions = {
  showSymbol: boolean;
  actions?: PositionActionsConfig;
};

export function makePositionColumns(
  marketId: string,
  options: MakePositionColumnsOptions,
): ExtendedColumnDef<Position>[] {
  const { showSymbol, actions } = options;

  const columns: ExtendedColumnDef<Position>[] = [];

  if (showSymbol) {
    columns.push({
      id: "symbolName",
      header: "Symbol",
      accessorFn: (row) => row.symbolName,
      cell: ({ row }) => (
        <Link
          href={`/plutus/${marketId}/${row.original.symbolId}`}
          className="hover:underline"
        >
          {row.original.symbolName}
        </Link>
      ),
      filterConfig: {
        type: "string",
        operators: ["eq", "neq", "contains", "startsWith", "endsWith"],
      },
    });
  }

  columns.push(
    {
      id: "side",
      header: "Side",
      accessorFn: (row) => row.side,
      cell: ({ getValue }) => {
        const side = getValue<string>();
        return (
          <span
            className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${positionSideColors[side] ?? ""}`}
          >
            {side}
          </span>
        );
      },
      filterConfig: {
        type: "enum",
        operators: ["eq", "neq"],
        enumValues: ["Buy", "Sell"],
      },
    },
    {
      id: "status",
      header: "Status",
      accessorFn: (row) => row.status,
      cell: ({ row }) => (
        <div className="flex items-center gap-1">
          <PositionStatusBadge status={row.original.status} />
          {row.original.linkedBuyPositionId && <LinkedBadge />}
        </div>
      ),
      filterConfig: {
        type: "enum",
        operators: ["eq", "neq"],
        enumValues: ["Pending", "DidNotBuy", "Bought", "DidNotSell", "Sold"],
      },
    },
    {
      id: "cost",
      header: "Cost",
      accessorFn: (row) => row.cost,
      cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "quantity",
      header: "Quantity",
      accessorFn: (row) => row.quantity,
      cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
      filterConfig: {
        type: "number",
        operators: ["eq", "neq", "gt", "gte", "lt", "lte"],
      },
    },
    {
      id: "total",
      header: "Total",
      accessorFn: (row) => row.cost * row.quantity,
      cell: ({ getValue }) => <PrettyNumber number={getValue<number>()} />,
      enableSorting: false,
    },
    {
      id: "createdAt",
      header: "Created",
      accessorFn: (row) => row.createdAt,
      cell: ({ getValue }) => new Date(getValue<string>()).toLocaleDateString(),
    },
  );

  if (actions) {
    columns.push({
      id: "actions",
      header: "",
      accessorFn: () => "",
      enableSorting: false,
      cell: ({ row }) => (
        <PositionActions
          position={row.original}
          variant={actions.variant}
          onEdit={actions.onEdit}
          onClose={actions.onClose}
          onLink={actions.onLink}
          onCreateSell={actions.onCreateSell}
        />
      ),
    });
  }

  return columns;
}
