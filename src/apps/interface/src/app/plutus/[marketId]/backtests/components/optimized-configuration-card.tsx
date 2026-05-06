import { Typography } from "@/app/components/typography";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { type StrategyConfiguration } from "@/lib/api/plutus";
import { ArrowRight } from "lucide-react";
import { signalTypeLabels } from "../../strategies/components/strategy-constants";

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
  configuration,
  isApplying,
  onApplyToStrategy,
}: {
  configuration: StrategyConfiguration;
  isApplying?: boolean;
  onApplyToStrategy?: () => void;
}) {
  const hasSignalWeights =
    configuration.signalWeights != null &&
    configuration.signalWeights.length > 0;

  const scalarFields: Array<{
    label: string;
    key: keyof StrategyConfiguration;
  }> = [
    { label: "Buy Threshold", key: "buyThreshold" },
    { label: "Sell Threshold", key: "sellThreshold" },
    { label: "Max Positions", key: "maxPositions" },
    { label: "Max Position Percent", key: "maxPositionPercent" },
    { label: "Hold Period Days", key: "holdPeriodDays" },
    { label: "Forecast Movement Threshold", key: "forecastMovementThreshold" },
    { label: "Forecast Horizon Days", key: "forecastHorizonDays" },
    { label: "Deviation Multiplier", key: "deviationMultiplier" },
    { label: "Mean Time Frame Value", key: "meanTimeFrameValue" },
    { label: "Min Margin Percent", key: "minMarginPercent" },
  ];

  const hasComponents =
    configuration.components != null && configuration.components.length > 0;

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
          {hasSignalWeights && (
            <div className="space-y-2">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Signal Weights
              </Typography>
              {configuration.signalWeights!.map((sw, i) => (
                <ConfigRow
                  key={i}
                  label={signalTypeLabels[sw.type] ?? sw.type}
                  value={sw.weight}
                />
              ))}
            </div>
          )}
          <div className="space-y-1">
            <Typography
              variant="small"
              className="font-semibold uppercase tracking-wide text-muted-foreground"
            >
              Parameters
            </Typography>
            {scalarFields.map(
              (field) =>
                configuration[field.key] != null && (
                  <ConfigRow
                    key={field.key}
                    label={field.label}
                    value={configuration[field.key] as string | number}
                  />
                ),
            )}
          </div>
          {hasComponents && (
            <div className="space-y-2">
              <Typography
                variant="small"
                className="font-semibold uppercase tracking-wide text-muted-foreground"
              >
                Components
              </Typography>
              {configuration.components!.map((c, i) => (
                <ConfigRow
                  key={i}
                  label={c.strategyId}
                  value={`Weight: ${c.weight}`}
                />
              ))}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
