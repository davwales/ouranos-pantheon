import { abbreviateNumber } from "@/components/shared/pretty-number/abbreviate-number";
import { type ExtendedColumnDef } from "@/components/shared/responsive-data-table";
import { Button } from "@/components/ui/button";
import { type StrategyRecommendation } from "@/lib/api/plutus";
import { Plus } from "lucide-react";
import { OriginLink } from "@/components/shared/breadcrumbs";
import { RecommendationDetailsPopover } from "./recommendation-details-popover";
import { ScoreBar } from "./score-bar";

type Column = ExtendedColumnDef<StrategyRecommendation>;

function formatUnits(units: number) {
  return units.toLocaleString("en-US");
}

function symbolColumn(marketId: string): Column {
  return {
    id: "symbol",
    header: "Symbol",
    accessorFn: (row) => row.symbolName,
    cell: ({ row }) => (
      <div className="flex items-center gap-1.5 min-w-0">
        <OriginLink
          href={`/plutus/${marketId}/${row.original.symbolId}`}
          className="font-medium hover:underline truncate"
        >
          {row.original.symbolName}
          {row.original.symbolSubcode ? ` (${row.original.symbolSubcode})` : ""}
        </OriginLink>
        <RecommendationDetailsPopover recommendation={row.original} />
      </div>
    ),
  };
}

function actionColumn(
  onCreatePosition: (recommendation: StrategyRecommendation) => void,
): Column {
  return {
    id: "actions",
    header: "",
    accessorFn: () => "",
    cell: ({ row }) => (
      <Button
        variant="ghost"
        size="icon"
        className="h-7 w-7"
        aria-label={`Create position for ${row.original.symbolName}`}
        onClick={() => onCreatePosition(row.original)}
      >
        <Plus className="w-4 h-4" />
      </Button>
    ),
  };
}

export function makeSwingRecommendationColumns(
  marketId: string,
  onCreatePosition: (recommendation: StrategyRecommendation) => void,
): Column[] {
  return [
    symbolColumn(marketId),
    {
      id: "buy",
      header: "Buy",
      accessorFn: (row) => row.suggestedVolume,
      cell: ({ row }) => (
        <span className="tabular-nums">
          {formatUnits(row.original.suggestedVolume)} @{" "}
          {abbreviateNumber(row.original.currentPrice)}
        </span>
      ),
    },
    {
      id: "allocation",
      header: "Allocation",
      accessorFn: (row) => row.suggestedAllocation,
      cell: ({ getValue }) => (
        <span className="tabular-nums">
          {abbreviateNumber(getValue<number>())}
        </span>
      ),
    },
    {
      id: "score",
      header: "Score",
      accessorFn: (row) => row.score,
      cell: ({ getValue }) => <ScoreBar score={getValue<number>()} />,
    },
    actionColumn(onCreatePosition),
  ];
}

export function makeFlipRecommendationColumns(
  marketId: string,
  onCreatePosition: (recommendation: StrategyRecommendation) => void,
): Column[] {
  return [
    symbolColumn(marketId),
    {
      id: "prices",
      header: "Bid → Ask",
      accessorFn: (row) => row.buyPrice,
      cell: ({ row }) => (
        <span className="tabular-nums">
          {abbreviateNumber(row.original.buyPrice ?? 0)} →{" "}
          {abbreviateNumber(row.original.sellPrice ?? 0)}
        </span>
      ),
    },
    {
      id: "units",
      header: "Units",
      accessorFn: (row) => row.suggestedVolume,
      cell: ({ getValue }) => (
        <span className="tabular-nums">{formatUnits(getValue<number>())}</span>
      ),
    },
    {
      id: "margin",
      header: "Margin",
      accessorFn: (row) => row.netMargin,
      cell: ({ getValue }) => (
        <span className="tabular-nums">
          {((getValue<number | null>() ?? 0) * 100).toFixed(1)}%
        </span>
      ),
    },
    {
      id: "profit",
      header: "Est. Profit",
      accessorFn: (row) => row.expectedProfit,
      cell: ({ getValue }) => (
        <span className="tabular-nums text-green-600">
          {abbreviateNumber(getValue<number | null>() ?? 0)}
        </span>
      ),
    },
    actionColumn(onCreatePosition),
  ];
}
