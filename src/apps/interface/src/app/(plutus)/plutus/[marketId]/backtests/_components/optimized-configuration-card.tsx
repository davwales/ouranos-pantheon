import { Typography } from "@/components/shared/typography";
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
import { ConfigRow } from "../../strategies/_components/config-row";
import {
  INPUT_KIND_LABELS,
  THRESHOLD_FIELDS,
} from "../../strategies/_components/strategy-constants";

export function OptimizedConfigurationCard({
  results,
  isApplying,
  onApplyToStrategy,
}: {
  results: BacktestResults;
  isApplying?: boolean;
  onApplyToStrategy?: () => void;
}) {
  const trading = results.optimizedConfiguration;
  const optimizedWeights = results.optimizedInputWeights
    ? [...results.optimizedInputWeights].sort((a, b) => b.weight - a.weight)
    : [];
  const hasThresholds = THRESHOLD_FIELDS.some(
    (f) => results.optimizedThresholds?.[f.key] != null,
  );
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
          {trading && (
            <div className="space-y-1">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Trading Rules
              </Typography>
              <ConfigRow label="Max Positions" value={trading.maxPositions} />
              <ConfigRow
                label="Max Position Percent"
                value={trading.maxPositionPercent}
              />
              <ConfigRow
                label="Hold Period Days"
                value={trading.holdPeriodDays}
              />
            </div>
          )}

          {optimizedWeights.length > 0 && (
            <div className="space-y-2">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Optimized Input Weights
              </Typography>
              <div className="space-y-1">
                {optimizedWeights.map((w) => (
                  <ConfigRow
                    key={w.kind}
                    label={INPUT_KIND_LABELS[w.kind]}
                    value={w.weight}
                  />
                ))}
              </div>
            </div>
          )}

          {hasThresholds && results.optimizedThresholds && (
            <div className="space-y-2">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Optimized Thresholds
              </Typography>
              <div className="space-y-1">
                {THRESHOLD_FIELDS.map((f) => (
                  <ConfigRow
                    key={f.key}
                    label={f.label}
                    value={results.optimizedThresholds?.[f.key]}
                  />
                ))}
              </div>
            </div>
          )}

          {outSample && (
            <div className="space-y-2">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Out-of-Sample
              </Typography>
              <div className="space-y-1">
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
              </div>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
