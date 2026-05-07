import { abbreviateNumber } from "@/app/components/pretty-number";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { BacktestDetail, StrategyDetail } from "@/lib/api/plutus";
import { Play, RefreshCw } from "lucide-react";
import Link from "next/link";
import { StatusChip } from "./status-chip";

export function BacktestResultHeader({
  backtest,
  marketId,
  strategy,
  onRefresh,
  onRunAgain,
}: {
  backtest: BacktestDetail;
  marketId: string;
  strategy: StrategyDetail | null | undefined;
  onRefresh: () => void;
  onRunAgain?: () => void;
}) {
  return (
    <Card className="border-l-4">
      <CardContent className="pt-6 pb-6">
        <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
          <div className="space-y-3 min-w-0">
            <div className="flex items-center gap-3">
              <div className="min-w-0">
                <h2 className="text-2xl font-semibold tracking-tight">
                  {backtest.kind === "Optimization"
                    ? "Optimization Results"
                    : "Backtest Results"}
                </h2>
                <p className="text-sm text-muted-foreground">
                  {strategy ? (
                    <Link
                      href={`/plutus/${marketId}/strategies/${strategy.id}`}
                      className="hover:underline"
                    >
                      {strategy.name}
                    </Link>
                  ) : (
                    "Strategy"
                  )}
                </p>
              </div>
            </div>

            <div className="flex flex-wrap gap-2">
              <span className="inline-flex items-center gap-1 rounded-full border bg-muted/50 px-2.5 py-1 text-xs font-medium">
                <span className="text-muted-foreground">Budget:</span>
                {abbreviateNumber(backtest.budget)}
              </span>
              <span className="inline-flex items-center gap-1 rounded-full border bg-muted/50 px-2.5 py-1 text-xs font-medium">
                <span className="text-muted-foreground">Range:</span>
                {new Date(backtest.startDate).toLocaleDateString()} -{" "}
                {new Date(backtest.endDate).toLocaleDateString()}
              </span>
              <StatusChip status={backtest.status} />
              <span
                className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                  backtest.kind === "Optimization"
                    ? "bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400"
                    : "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-400"
                }`}
              >
                {backtest.kind}
              </span>
            </div>
          </div>

          <div className="flex items-center gap-3 shrink-0">
            {onRunAgain && (
              <Button variant="outline" size="sm" onClick={onRunAgain}>
                <Play className="w-4 h-4 mr-1" />
                Run Again
              </Button>
            )}
            <Button variant="outline" size="sm" onClick={onRefresh}>
              <RefreshCw className="w-4 h-4 mr-1" />
              Refresh
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
