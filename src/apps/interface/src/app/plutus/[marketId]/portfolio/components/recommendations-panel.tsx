"use client";

import { Button } from "@/components/ui/button";
import { useApi } from "@/hooks/use-api";
import {
  type Strategy,
  type StrategyRecommendation,
  plutusApi,
} from "@/lib/api/plutus";
import { Lightbulb, RefreshCw } from "lucide-react";
import { useState } from "react";

export function RecommendationsPanel({
  marketId,
  strategies,
  onCreatePosition,
}: {
  marketId: string;
  strategies: Strategy[];
  onCreatePosition: (symbolId: string, symbolName: string) => void;
}) {
  const [selectedStrategyId, setSelectedStrategyId] = useState<string | null>(
    null,
  );

  const [recommendationsState, reexecuteRecommendations] = useApi(
    () =>
      selectedStrategyId
        ? plutusApi.getRecommendations(selectedStrategyId, {
            marketId,
            budget: 10000,
          })
        : Promise.resolve({ recommendations: [] }),
    [selectedStrategyId, marketId],
  );

  const recommendations = recommendationsState.data?.recommendations ?? [];
  const loading = recommendationsState.status === "loading";

  const activeStrategies = strategies.filter((s) => s.isActive);

  if (activeStrategies.length === 0) {
    return null;
  }

  return (
    <div className="mt-6 rounded-lg border bg-card p-4">
      <div className="flex items-center gap-2 mb-3">
        <Lightbulb className="w-5 h-5 text-yellow-500" />
        <h3 className="font-semibold">Strategy Recommendations</h3>
      </div>

      <div className="flex flex-wrap gap-2 mb-4">
        {activeStrategies.map((strategy) => (
          <Button
            key={strategy.id}
            variant={selectedStrategyId === strategy.id ? "default" : "outline"}
            size="sm"
            onClick={() =>
              setSelectedStrategyId(
                selectedStrategyId === strategy.id ? null : strategy.id,
              )
            }
          >
            {strategy.name}
          </Button>
        ))}
      </div>

      {selectedStrategyId && (
        <>
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm text-muted-foreground">
              {recommendations.length} recommendation
              {recommendations.length !== 1 ? "s" : ""}
            </span>
            {loading ? (
              <RefreshCw className="w-4 h-4 animate-spin" />
            ) : (
              <RefreshCw
                onClick={reexecuteRecommendations}
                className="w-4 h-4 hover:cursor-pointer"
              />
            )}
          </div>

          {recommendations.length > 0 && (
            <div className="divide-y">
              {recommendations.map((rec: StrategyRecommendation) => (
                <RecommendationRow
                  key={rec.symbolId}
                  recommendation={rec}
                  onCreatePosition={onCreatePosition}
                />
              ))}
            </div>
          )}

          {recommendations.length === 0 && !loading && (
            <p className="text-sm text-muted-foreground py-2">
              No recommendations found for the selected strategy.
            </p>
          )}
        </>
      )}

      {!selectedStrategyId && (
        <p className="text-sm text-muted-foreground">
          Select a strategy to view buy/sell recommendations for this market.
        </p>
      )}
    </div>
  );
}

function RecommendationRow({
  recommendation,
  onCreatePosition,
}: {
  recommendation: StrategyRecommendation;
  onCreatePosition: (symbolId: string, symbolName: string) => void;
}) {
  return (
    <div className="flex items-center justify-between py-2 first:pt-0 last:pb-0">
      <div className="flex flex-col">
        <span className="font-medium">
          {recommendation.symbolName}
          {recommendation.symbolSubcode
            ? ` (${recommendation.symbolSubcode})`
            : ""}
        </span>
        <span className="text-xs text-muted-foreground">
          Score: {recommendation.score.toFixed(3)} &middot; Price:{" "}
          {recommendation.currentPrice.toFixed(2)} &middot; Suggested:{" "}
          {recommendation.suggestedAllocation.toFixed(2)} (
          {recommendation.suggestedVolume} units)
        </span>
      </div>
      <Button
        variant="outline"
        size="sm"
        onClick={() =>
          onCreatePosition(recommendation.symbolId, recommendation.symbolName)
        }
      >
        Create Position
      </Button>
    </div>
  );
}
