import { abbreviateNumber } from "@/app/components/pretty-number";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { BacktestResults } from "@/lib/api/plutus";
import { StatCard } from "./stat-card";

export function BacktestStatisticsGrid({
  results,
}: {
  results: BacktestResults;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Additional Statistics</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
          <StatCard
            label="Total Trades"
            value={results.totalTrades.toString()}
          />
          <StatCard label="Winning" value={results.winningTrades.toString()} />
          <StatCard label="Losing" value={results.losingTrades.toString()} />
          <StatCard
            label="Best Trade"
            value={`${(results.bestTrade * 100).toFixed(1)}%`}
          />
          <StatCard
            label="Worst Trade"
            value={`${(results.worstTrade * 100).toFixed(1)}%`}
          />
          <StatCard
            label="Avg Return"
            value={`${(results.averageTradeReturn * 100).toFixed(2)}%`}
          />
          <StatCard
            label="Expectancy"
            value={
              results.expectancy != null
                ? abbreviateNumber(results.expectancy)
                : "-"
            }
          />
          <StatCard
            label="Final Balance"
            value={abbreviateNumber(results.finalBalance)}
          />
          <StatCard
            label="Total Return"
            value={abbreviateNumber(results.totalReturn)}
          />
          <StatCard
            label="Max Drawdown"
            value={abbreviateNumber(results.maxDrawdown)}
          />
        </div>
      </CardContent>
    </Card>
  );
}
