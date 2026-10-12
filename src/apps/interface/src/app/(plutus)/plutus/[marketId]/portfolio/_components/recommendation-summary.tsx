import { abbreviateNumber } from "@/components/shared/pretty-number/abbreviate-number";
import { type StrategyRecommendation } from "@/lib/api/plutus";

export function RecommendationSummary({
  recommendations,
  budget,
}: {
  recommendations: StrategyRecommendation[];
  budget: number;
}) {
  const allocated = recommendations.reduce(
    (sum, r) => sum + r.suggestedAllocation,
    0,
  );
  const profits = recommendations
    .map((r) => r.expectedProfit)
    .filter((p): p is number => p != null);

  return (
    <p className="text-sm text-muted-foreground">
      {recommendations.length} pick{recommendations.length === 1 ? "" : "s"} ·{" "}
      {abbreviateNumber(allocated)} of {abbreviateNumber(budget)} allocated
      {profits.length > 0 &&
        ` · est. profit ${abbreviateNumber(profits.reduce((a, b) => a + b, 0))}`}
    </p>
  );
}
