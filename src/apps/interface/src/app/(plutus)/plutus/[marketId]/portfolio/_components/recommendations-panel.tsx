"use client";

import { NumericInput } from "@/components/shared/numeric-input";
import { abbreviateNumber } from "@/components/shared/pretty-number/abbreviate-number";
import { Button } from "@/components/ui/button";
import {
  type Strategy,
  type StrategyRecommendation,
  plutusApi,
} from "@/lib/api/plutus";
import { Lightbulb, Sparkles } from "lucide-react";
import Link from "next/link";
import { useCallback, useRef, useState } from "react";

const DEFAULT_BUDGET = 10000;

type RecommendationsState =
  | { status: "idle"; data: undefined }
  | { status: "loading"; data: StrategyRecommendation[] | undefined }
  | { status: "success"; data: StrategyRecommendation[] }
  | {
      status: "error";
      data: StrategyRecommendation[] | undefined;
      error: Error;
    };

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
  const [budget, setBudget] = useState(DEFAULT_BUDGET);
  const [recommendationsState, setRecommendationsState] =
    useState<RecommendationsState>({ status: "idle", data: undefined });
  const abortRef = useRef<AbortController | null>(null);

  const handleGenerate = useCallback(async () => {
    if (!selectedStrategyId) {
      return;
    }

    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setRecommendationsState((prev) => ({
      status: "loading",
      data: prev.data,
    }));

    try {
      const result = await plutusApi.getRecommendations(selectedStrategyId, {
        marketId,
        budget,
      });
      if (!controller.signal.aborted) {
        setRecommendationsState({
          status: "success",
          data: result.recommendations,
        });
      }
    } catch (error) {
      if (!controller.signal.aborted) {
        setRecommendationsState((prev) => ({
          status: "error",
          data: prev.data,
          error: error instanceof Error ? error : new Error(String(error)),
        }));
      }
    }
  }, [selectedStrategyId, marketId, budget]);

  const recommendations = recommendationsState.data ?? [];
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

      <div className="flex flex-col sm:flex-row sm:flex-wrap items-stretch sm:items-center gap-2 mb-4">
        {activeStrategies.map((strategy) => {
          const isSelected = selectedStrategyId === strategy.id;
          return (
            <Button
              key={strategy.id}
              variant={isSelected ? "default" : "outline"}
              size="sm"
              className="w-full sm:w-auto"
              onClick={() => {
                const newId = isSelected ? null : strategy.id;
                setSelectedStrategyId(newId);
                setRecommendationsState({ status: "idle", data: undefined });
              }}
            >
              {strategy.name}
            </Button>
          );
        })}
        {selectedStrategyId && (
          <div className="flex items-center gap-1.5 w-full sm:w-auto sm:ml-auto">
            <label
              htmlFor="budget-input"
              className="text-xs text-muted-foreground whitespace-nowrap"
            >
              Budget:
            </label>
            <NumericInput
              id="budget-input"
              value={budget}
              onChange={(v) => setBudget(v ?? DEFAULT_BUDGET)}
              min={1}
              className="w-full sm:w-36 h-7 text-sm"
            />
          </div>
        )}
      </div>

      {selectedStrategyId && (
        <>
          <div className="flex items-center justify-stretch sm:justify-end mb-2">
            <Button
              variant="outline"
              size="sm"
              className="w-full sm:w-auto"
              onClick={handleGenerate}
              disabled={loading || !selectedStrategyId}
            >
              <Sparkles className="w-3.5 h-3.5 mr-1.5" />
              {loading ? "Generating..." : "Generate"}
            </Button>
          </div>

          {recommendations.length > 0 && (
            <div className="divide-y">
              {recommendations.map((rec: StrategyRecommendation) => (
                <RecommendationRow
                  key={rec.symbolId}
                  marketId={marketId}
                  recommendation={rec}
                  onCreatePosition={onCreatePosition}
                />
              ))}
            </div>
          )}

          {recommendations.length === 0 && !loading && (
            <p className="text-sm text-muted-foreground py-2">
              {recommendationsState.status === "idle"
                ? "Set your budget and click Generate to get recommendations."
                : "No recommendations found for the selected strategy."}
            </p>
          )}
        </>
      )}

      {!selectedStrategyId && (
        <p className="text-sm text-muted-foreground">
          Select a strategy to generate buy/sell recommendations for this
          market.
        </p>
      )}
    </div>
  );
}

function RecommendationRow({
  marketId,
  recommendation,
  onCreatePosition,
}: {
  marketId: string;
  recommendation: StrategyRecommendation;
  onCreatePosition: (symbolId: string, symbolName: string) => void;
}) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 py-2 first:pt-0 last:pb-0">
      <div className="flex flex-col gap-0.5 min-w-0">
        <Link
          href={`/plutus/${marketId}/${recommendation.symbolId}`}
          className="font-medium hover:underline truncate"
        >
          {recommendation.symbolName}
          {recommendation.symbolSubcode
            ? ` (${recommendation.symbolSubcode})`
            : ""}
        </Link>
        <div className="flex flex-wrap gap-x-2 text-xs text-muted-foreground">
          <span>Score: {recommendation.score.toFixed(3)}</span>
          <span className="hidden sm:inline">&middot;</span>
          <span>Price: {abbreviateNumber(recommendation.currentPrice)}</span>
          <span className="hidden sm:inline">&middot;</span>
          <span>
            Allocation: {abbreviateNumber(recommendation.suggestedAllocation)}
          </span>
          <span className="hidden sm:inline">&middot;</span>
          <span>{recommendation.suggestedVolume} units</span>
        </div>
        {recommendation.rationale && (
          <span className="text-xs text-muted-foreground italic">
            {recommendation.rationale}
          </span>
        )}
      </div>
      <Button
        variant="outline"
        size="sm"
        className="w-full sm:w-auto shrink-0"
        onClick={() =>
          onCreatePosition(recommendation.symbolId, recommendation.symbolName)
        }
      >
        Create Position
      </Button>
    </div>
  );
}
