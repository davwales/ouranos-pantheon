"use client";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import { Typography } from "@/components/shared/typography";
import {
  type Strategy,
  type StrategyRecommendation,
  plutusApi,
} from "@/lib/api/plutus";
import { useCallback, useMemo, useRef, useState } from "react";
import {
  makeFlipRecommendationColumns,
  makeSwingRecommendationColumns,
} from "./recommendation-columns";
import { RecommendationSummary } from "./recommendation-summary";
import { RecommendationsToolbar } from "./recommendations-toolbar";

const DEFAULT_BUDGET = 10000;

type RecommendationsState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "success"; data: StrategyRecommendation[]; budget: number }
  | { status: "error"; error: Error };

export function RecommendationsPanel({
  marketId,
  strategies,
  onCreatePosition,
}: {
  marketId: string;
  strategies: Strategy[];
  onCreatePosition: (symbolId: string, symbolName: string) => void;
}) {
  const activeStrategies = useMemo(
    () => strategies.filter((s) => s.isActive),
    [strategies],
  );
  const [strategyId, setStrategyId] = useState(activeStrategies[0]?.id ?? "");
  const [budget, setBudget] = useState(DEFAULT_BUDGET);
  const [state, setState] = useState<RecommendationsState>({ status: "idle" });
  const abortRef = useRef<AbortController | null>(null);

  const handleGenerate = useCallback(async () => {
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;
    setState({ status: "loading" });

    try {
      const result = await plutusApi.getRecommendations(strategyId, {
        marketId,
        budget,
      });
      if (!controller.signal.aborted) {
        setState({ status: "success", data: result.recommendations, budget });
      }
    } catch (error) {
      if (!controller.signal.aborted) {
        setState({
          status: "error",
          error: error instanceof Error ? error : new Error(String(error)),
        });
      }
    }
  }, [strategyId, marketId, budget]);

  const recommendations = state.status === "success" ? state.data : [];
  const isFlip = recommendations.some((r) => r.buyPrice != null);
  const columns = useMemo(
    () =>
      isFlip
        ? makeFlipRecommendationColumns(marketId, onCreatePosition)
        : makeSwingRecommendationColumns(marketId, onCreatePosition),
    [isFlip, marketId, onCreatePosition],
  );

  if (activeStrategies.length === 0) {
    return null;
  }

  return (
    <section className="mt-8 space-y-3">
      <Typography variant="h4">Recommendations</Typography>
      <RecommendationsToolbar
        strategies={activeStrategies}
        strategyId={strategyId}
        onStrategyChange={(id) => {
          setStrategyId(id);
          setState({ status: "idle" });
        }}
        budget={budget}
        onBudgetChange={setBudget}
        loading={state.status === "loading"}
        onGenerate={handleGenerate}
      />
      <RecommendationsResult
        state={state}
        columns={columns}
        recommendations={recommendations}
      />
    </section>
  );
}

function RecommendationsResult({
  state,
  columns,
  recommendations,
}: {
  state: RecommendationsState;
  columns: ReturnType<typeof makeSwingRecommendationColumns>;
  recommendations: StrategyRecommendation[];
}) {
  if (state.status === "idle") {
    return (
      <p className="text-sm text-muted-foreground">
        Pick a strategy and budget, then generate to see what it would buy now.
      </p>
    );
  }

  if (state.status === "error") {
    return <p className="text-sm text-destructive">{state.error.message}</p>;
  }

  if (state.status === "success" && recommendations.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        Nothing meets this strategy’s criteria right now.
      </p>
    );
  }

  return (
    <div className="space-y-2">
      {state.status === "success" && (
        <RecommendationSummary
          recommendations={recommendations}
          budget={state.budget}
        />
      )}
      <ResponsiveDataTable
        columns={columns}
        data={recommendations}
        loading={state.status === "loading"}
        disablePagination
        disableSorting
        disableFiltering
        className="overflow-hidden"
      />
    </div>
  );
}
