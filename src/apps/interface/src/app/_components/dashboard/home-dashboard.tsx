"use client";

import { CardSkeleton } from "@/components/shared/skeletons";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import { ActiveBacktestsView } from "./active-backtests-view";
import { MarketMoversView } from "./market-movers-view";
import { OpenPositionsView } from "./open-positions-view";
import { RecentConversationsView } from "./recent-conversations-view";
import { RecentRecipesView } from "./recent-recipes-view";
import { ShoppingListView } from "./shopping-list-view";

export function HomeDashboard() {
  const [marketsState] = useApi(() => plutusApi.getAllMarkets(), []);
  const markets = marketsState.data;

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
      {markets ? (
        <>
          <MarketMoversView markets={markets} />
          <OpenPositionsView markets={markets} />
          <ActiveBacktestsView markets={markets} />
        </>
      ) : (
        <>
          <CardSkeleton />
          <CardSkeleton />
          <CardSkeleton />
        </>
      )}
      <RecentConversationsView />
      <RecentRecipesView />
      <ShoppingListView />
    </div>
  );
}
