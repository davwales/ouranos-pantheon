"use client";

import { abbreviateNumber } from "@/components/shared/pretty-number";
import { Badge } from "@/components/ui/badge";
import { useApi } from "@/hooks/use-api";
import { type Market, type Position, plutusApi } from "@/lib/api/plutus";
import { Briefcase } from "lucide-react";
import { DashboardCard } from "./dashboard-card";
import { DashboardRow } from "./dashboard-row";

const POSITIONS_SHOWN = 5;
const OPEN_STATUSES_FILTER = "status:in:Pending,Bought";

type OpenPositions = {
  positions: Position[];
  totalCount: number;
};

export function OpenPositionsView({ markets }: { markets: Market[] }) {
  const marketNames = new Map(markets.map((m) => [m.id, m.name]));

  const [state] = useApi(async (): Promise<OpenPositions> => {
    const pages = await Promise.all(
      markets.map((market) =>
        plutusApi.getAllPositions(market.id, {
          filter: [OPEN_STATUSES_FILTER],
          sortField: "CreatedAt",
          sortDirection: "desc",
          take: POSITIONS_SHOWN,
        }),
      ),
    );
    return {
      positions: pages
        .flatMap((page) => page.items)
        .sort((a, b) => b.createdAt.localeCompare(a.createdAt))
        .slice(0, POSITIONS_SHOWN),
      totalCount: pages.reduce((sum, page) => sum + page.totalCount, 0),
    };
  }, [markets.map((m) => m.id)]);

  return (
    <DashboardCard
      title={
        state.data ? `Open Positions (${state.data.totalCount})` : "Open Positions"
      }
      icon={Briefcase}
      state={state}
      isEmpty={(data) => data.positions.length === 0}
      emptyText="No pending or held positions."
    >
      {(data) => (
        <ul>
          {data.positions.map((position) => (
            <DashboardRow
              key={position.id}
              href={`/plutus/${position.marketId}/portfolio`}
              primary={position.symbolName}
              secondary={`${marketNames.get(position.marketId) ?? ""} · ${abbreviateNumber(position.quantity, 0)} @ ${abbreviateNumber(position.cost)}`}
              trailing={
                <div className="flex items-center gap-1">
                  <Badge variant="outline">{position.side}</Badge>
                  <Badge variant="secondary">{position.status}</Badge>
                </div>
              }
            />
          ))}
        </ul>
      )}
    </DashboardCard>
  );
}
