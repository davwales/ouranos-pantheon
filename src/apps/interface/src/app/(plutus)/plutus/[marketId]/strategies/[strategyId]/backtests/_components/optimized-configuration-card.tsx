import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { type BacktestResults } from "@/lib/api/plutus";
import { ArrowRight } from "lucide-react";
import { ConfigGroup } from "../../../_components/config-group";
import { ConfigRow } from "../../../_components/config-row";
import { StrategyConfigurationView } from "../../../_components/strategy-configuration-view";

export function OptimizedConfigurationCard({
  results,
  isApplying,
  onApplyToStrategy,
}: {
  results: BacktestResults;
  isApplying?: boolean;
  onApplyToStrategy?: () => void;
}) {
  const outSample = results.outSampleResults;

  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between gap-4">
          <div>
            <CardTitle>Optimized Configuration</CardTitle>
            <CardDescription>
              These parameters were found by the genetic algorithm optimizer
            </CardDescription>
          </div>
          <div className="flex flex-col items-end gap-2 shrink-0">
            <Badge
              className={
                results.isValidated
                  ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400 border-transparent"
                  : "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400 border-transparent"
              }
            >
              {results.isValidated ? "Validated" : "Not Validated"}
            </Badge>
            {onApplyToStrategy && (
              <Button
                onClick={onApplyToStrategy}
                disabled={isApplying}
                size="sm"
              >
                <ArrowRight className="w-4 h-4 mr-1" />
                {isApplying ? "Applying..." : "Apply to Strategy"}
              </Button>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {results.optimizedConfiguration && (
            <StrategyConfigurationView
              configuration={results.optimizedConfiguration}
            />
          )}

          {outSample && (
            <ConfigGroup title="Out-of-Sample">
              <ConfigRow
                label="Sharpe Ratio"
                value={outSample.sharpeRatio.toFixed(2)}
              />
              <ConfigRow
                label="Total Return %"
                value={`${(outSample.totalReturnPercent * 100).toFixed(2)}%`}
              />
              <ConfigRow
                label="Max Drawdown %"
                value={`${(outSample.maxDrawdownPercent * 100).toFixed(2)}%`}
              />
            </ConfigGroup>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
