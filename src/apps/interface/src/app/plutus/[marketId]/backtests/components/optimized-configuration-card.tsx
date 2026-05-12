import { Typography } from "@/app/components/typography";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { BacktestResults } from "@/lib/api/plutus";
import { ArrowRight } from "lucide-react";
import { signalWeightFields } from "../../strategies/components/strategy-constants";

function ConfigRow({
  label,
  value,
}: {
  label: string;
  value: string | number | null | undefined;
}) {
  if (value == null) {
    return null;
  }
  return (
    <div className="flex justify-between items-center py-2 border-b border-border/50 last:border-b-0">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className="text-sm font-medium">{String(value)}</span>
    </div>
  );
}

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
  const signalWeighted = results.optimizedSignalWeightedConfig;
  const forecastMomentum = results.optimizedForecastMomentumConfig;
  const meanReversion = results.optimizedMeanReversionConfig;
  const recipeArbitrage = results.optimizedRecipeArbitrageConfig;

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Optimized Configuration</CardTitle>
            <CardDescription>
              These parameters were found by the genetic algorithm optimizer
            </CardDescription>
          </div>
          {onApplyToStrategy && (
            <Button onClick={onApplyToStrategy} disabled={isApplying} size="sm">
              <ArrowRight className="w-4 h-4 mr-1" />
              {isApplying ? "Applying..." : "Apply to Strategy"}
            </Button>
          )}
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

          {signalWeighted && (
            <div className="space-y-2">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Signal Weights
              </Typography>
              {signalWeightFields.map(
                (field) =>
                  signalWeighted[field.key] != null && (
                    <ConfigRow
                      key={field.key}
                      label={field.label as string}
                      value={signalWeighted[field.key] as number}
                    />
                  ),
              )}
              <div className="mt-2 space-y-1">
                <ConfigRow
                  label="Buy Threshold"
                  value={signalWeighted.buyThreshold}
                />
                <ConfigRow
                  label="Sell Threshold"
                  value={signalWeighted.sellThreshold}
                />
              </div>
            </div>
          )}

          {forecastMomentum && (
            <div className="space-y-1">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Forecast Parameters
              </Typography>
              <ConfigRow
                label="Forecast Movement Threshold"
                value={forecastMomentum.forecastMovementThreshold}
              />
              <ConfigRow
                label="Forecast Horizon Days"
                value={forecastMomentum.forecastHorizonDays}
              />
            </div>
          )}

          {meanReversion && (
            <div className="space-y-1">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Mean Reversion Parameters
              </Typography>
              <ConfigRow
                label="Deviation Multiplier"
                value={meanReversion.deviationMultiplier}
              />
              <ConfigRow
                label="Mean Time Frame Value"
                value={meanReversion.meanTimeFrameValue}
              />
            </div>
          )}

          {recipeArbitrage && (
            <div className="space-y-1">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Recipe Arbitrage Parameters
              </Typography>
              <ConfigRow
                label="Min Margin Percent"
                value={recipeArbitrage.minMarginPercent}
              />
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
