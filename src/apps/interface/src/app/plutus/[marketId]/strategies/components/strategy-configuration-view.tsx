import { Typography } from "@/app/components/typography";
import { type StrategyDetail } from "@/lib/api/plutus";
import { ConfigRow } from "./config-row";
import { signalTypeLabels, strategyTypeLabels } from "./strategy-constants";

export function StrategyConfigurationView({
  configuration,
}: {
  configuration: StrategyDetail["configuration"];
}) {
  const signalTypes = [
    "TaxAdjustedRoi",
    "VolumeAnomaly",
    "TrendMomentum",
    "BollingerBands",
    "Rsi",
    "MovingAverageCrossover",
    "PriceVelocity",
  ];

  const weights =
    configuration.signalWeights && configuration.signalWeights.length > 0
      ? configuration.signalWeights
      : signalTypes.map((t) => ({ type: t, weight: 1 }));

  return (
    <div className="space-y-4">
      {weights.length > 0 && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Signal Weights
          </Typography>
          {weights.map((w, i) => (
            <ConfigRow
              key={i}
              label={signalTypeLabels[w.type] ?? w.type}
              value={w.weight}
            />
          ))}
        </div>
      )}
      <div>
        <Typography
          variant="small"
          className="font-semibold uppercase tracking-wide text-muted-foreground"
        >
          Parameters
        </Typography>
        <div className="mt-2 space-y-1">
          <ConfigRow label="Buy Threshold" value={configuration.buyThreshold} />
          <ConfigRow
            label="Sell Threshold"
            value={configuration.sellThreshold}
          />
          <ConfigRow
            label="Forecast Movement Threshold"
            value={configuration.forecastMovementThreshold}
          />
          <ConfigRow
            label="Forecast Horizon Days"
            value={configuration.forecastHorizonDays}
          />
          <ConfigRow
            label="Deviation Multiplier"
            value={configuration.deviationMultiplier}
          />
          <ConfigRow
            label="Mean Time Frame Value"
            value={configuration.meanTimeFrameValue}
          />
          <ConfigRow
            label="Min Margin Percent"
            value={configuration.minMarginPercent}
          />
          <ConfigRow label="Max Positions" value={configuration.maxPositions} />
          <ConfigRow
            label="Max Position Percent"
            value={configuration.maxPositionPercent}
          />
          <ConfigRow
            label="Hold Period Days"
            value={configuration.holdPeriodDays}
          />
        </div>
      </div>
      {configuration.components && configuration.components.length > 0 && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Components
          </Typography>
          {configuration.components.map((c, i) => (
            <ConfigRow
              key={i}
              label={strategyTypeLabels[c.type] ?? c.type}
              value={`Weight: ${c.weight}`}
            />
          ))}
        </div>
      )}
    </div>
  );
}
