"use client";

import { MarketAnalystButton } from "@/app/(plutus)/plutus/[marketId]/_components/market-analyst-button";
import { useNavBarActions } from "@/components/shared/nav-bar-actions-context";
import { usePlutusStore } from "@/stores/plutus-store";
import { useEffect } from "react";

const SUGGESTIONS = [
  "What are the best flips right now?",
  "How is the market trending?",
  "When is the market most active?",
];

export function MarketAnalystActions({ marketId }: { marketId: string }) {
  const timeFrameKey = usePlutusStore((state) => state.timeFrameKey);
  const { setActions, clearActions } = useNavBarActions();

  useEffect(() => {
    setActions(
      <MarketAnalystButton
        key={`${marketId}:${timeFrameKey}`}
        context={{ marketId, timeFrame: timeFrameKey }}
        suggestions={SUGGESTIONS}
      />,
    );
    return () => clearActions();
  }, [marketId, timeFrameKey, setActions, clearActions]);

  return null;
}
