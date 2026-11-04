"use client";

import ResponsiveDataTable from "@/components/shared/responsive-data-table/responsive-data-table";
import { Typography } from "@/components/shared/typography";
import { useApi } from "@/hooks/use-api";
import {
  type Strategy,
  type StrategyRecommendation,
  plutusApi,
} from "@/lib/api/plutus";
import { useCallback, useMemo, useState } from "react";
import {
  makeFlipRecommendationColumns,
  makeSwingRecommendationColumns,
} from "./recommendation-columns";
import { RecommendationSummary } from "./recommendation-summary";
import { RecommendationsToolbar } from "./recommendations-toolbar";

const DEFAULT_BUDGET = 10000;

type RecommendationsResultState =
  | { status: "loading" }
  | {
      status: "success";
      data: StrategyRecommendation[];
      budget: number;
      volumeParticipationRate: number;
    }
  | { status: "error"; error: Error };

function findLatestBacktested(strategies: Strategy[]) {
  return strategies.reduce<Strategy | undefined>((latest, strategy) => {
    if (!strategy.lastBacktestAt) return latest;
    if (!latest?.lastBacktestAt) return strategy;
    return strategy.lastBacktestAt > latest.lastBacktestAt ? strategy : latest;
  }, undefined);
}

function budgetFor(strategy: Strategy | undefined) {
  return strategy?.lastBacktestBudget ?? DEFAULT_BUDGET;
}

export function RecommendationsPanel({
  marketId,
  strategies,
  onCreatePosition,
}: {
  marketId: string;
  strategies: Strategy[];
  onCreatePosition: (
    recommendation: StrategyRecommendation,
    strategyId: string,
  ) => void;
}) {
  const activeStrategies = useMemo(
    () => strategies.filter((s) => s.isActive),
    [strategies],
  );

  if (activeStrategies.length === 0) {
    return null;
  }

  return (
    <ActiveRecommendationsPanel
      marketId={marketId}
      strategies={activeStrategies}
      onCreatePosition={onCreatePosition}
    />
  );
}

function ActiveRecommendationsPanel({
  marketId,
  strategies,
  onCreatePosition,
}: {
  marketId: string;
  strategies: Strategy[];
  onCreatePosition: (
    recommendation: StrategyRecommendation,
    strategyId: string,
  ) => void;
}) {
  const [strategyId, setStrategyId] = useState(
    () => (findLatestBacktested(strategies) ?? strategies[0]).id,
  );
  const selectedStrategy = strategies.find((s) => s.id === strategyId);
  const [budget, setBudget] = useState(() => budgetFor(selectedStrategy));
  const [submittedBudget, setSubmittedBudget] = useState(budget);

  const [apiState, regenerate] = useApi(
    async () => {
      const result = await plutusApi.getRecommendations(strategyId, {
        marketId,
        budget: submittedBudget,
      });
      return {
        strategyId,
        budget: submittedBudget,
        recommendations: result.recommendations,
        volumeParticipationRate: result.volumeParticipationRate,
      };
    },
    [strategyId, marketId, submittedBudget],
  );

  const isCurrent =
    apiState.data?.strategyId === strategyId &&
    apiState.data.budget === submittedBudget;

  const state: RecommendationsResultState =
    apiState.status === "error"
      ? { status: "error", error: apiState.error }
      : apiState.status === "success" && isCurrent
        ? {
            status: "success",
            data: apiState.data.recommendations,
            budget: apiState.data.budget,
            volumeParticipationRate: apiState.data.volumeParticipationRate,
          }
        : { status: "loading" };

  const handleStrategyChange = (id: string) => {
    const nextBudget = budgetFor(strategies.find((s) => s.id === id));
    setStrategyId(id);
    setBudget(nextBudget);
    setSubmittedBudget(nextBudget);
  };

  const handleGenerate = () => {
    if (budget === submittedBudget) {
      regenerate();
    } else {
      setSubmittedBudget(budget);
    }
  };

  const handleCreatePosition = useCallback(
    (recommendation: StrategyRecommendation) =>
      onCreatePosition(recommendation, strategyId),
    [onCreatePosition, strategyId],
  );

  const recommendations = state.status === "success" ? state.data : [];
  const isFlip = recommendations.some((r) => r.buyPrice != null);
  const columns = useMemo(
    () =>
      isFlip
        ? makeFlipRecommendationColumns(marketId, handleCreatePosition)
        : makeSwingRecommendationColumns(marketId, handleCreatePosition),
    [isFlip, marketId, handleCreatePosition],
  );

  return (
    <section className="mt-8 space-y-3">
      <Typography variant="h4">Recommendations</Typography>
      <RecommendationsToolbar
        strategies={strategies}
        strategyId={strategyId}
        onStrategyChange={handleStrategyChange}
        budget={budget}
        onBudgetChange={setBudget}
        loading={state.status === "loading"}
        onGenerate={handleGenerate}
      />
      {selectedStrategy?.lastBacktestAt && (
        <p className="text-xs text-muted-foreground">
          Budget and volume participation default to the latest completed backtest (
          {new Date(selectedStrategy.lastBacktestAt).toLocaleDateString()}).
        </p>
      )}
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
  state: RecommendationsResultState;
  columns: ReturnType<typeof makeSwingRecommendationColumns>;
  recommendations: StrategyRecommendation[];
}) {
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
          volumeParticipationRate={state.volumeParticipationRate}
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
