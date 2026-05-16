import { Typography } from "@/components/shared/typography";
import { type StrategyDetail } from "@/lib/api/plutus";
import { ConfigRow } from "./config-row";
import { signalWeightFields, strategyTypeLabels } from "./strategy-constants";

export function StrategyConfigurationView({ data }: { data: StrategyDetail }) {
  const config = data.signalWeightedConfig;

  return (
    <div className="space-y-4">
      <div className="space-y-1">
        <Typography
          variant="small"
          className="font-semibold uppercase tracking-wide text-muted-foreground"
        >
          Trading Rules
        </Typography>
        <div className="mt-2 space-y-1">
          <ConfigRow
            label="Max Positions"
            value={data.tradingConfiguration.maxPositions}
          />
          <ConfigRow
            label="Max Position Percent"
            value={data.tradingConfiguration.maxPositionPercent}
          />
          <ConfigRow
            label="Hold Period Days"
            value={data.tradingConfiguration.holdPeriodDays}
          />
        </div>
      </div>

      {config && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Signal Weights
          </Typography>
          {signalWeightFields.map(
            (field) =>
              config[field.key] != null && (
                <ConfigRow
                  key={field.key}
                  label={field.label as string}
                  value={config[field.key] as number}
                />
              ),
          )}
        </div>
      )}

      {config?.buyThreshold != null && (
        <ConfigRow label="Buy Threshold" value={config.buyThreshold} />
      )}
      {config?.sellThreshold != null && (
        <ConfigRow label="Sell Threshold" value={config.sellThreshold} />
      )}

      {data.forecastMomentumConfig && (
        <div className="space-y-1">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Forecast Parameters
          </Typography>
          <div className="mt-2 space-y-1">
            <ConfigRow
              label="Forecast Movement Threshold"
              value={data.forecastMomentumConfig.forecastMovementThreshold}
            />
            <ConfigRow
              label="Forecast Horizon Days"
              value={data.forecastMomentumConfig.forecastHorizonDays}
            />
          </div>
        </div>
      )}

      {data.meanReversionConfig && (
        <div className="space-y-1">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Mean Reversion Parameters
          </Typography>
          <div className="mt-2 space-y-1">
            <ConfigRow
              label="Deviation Multiplier"
              value={data.meanReversionConfig.deviationMultiplier}
            />
            <ConfigRow
              label="Mean Time Frame Value"
              value={data.meanReversionConfig.meanTimeFrameValue}
            />
          </div>
        </div>
      )}

      {data.recipeArbitrageConfig && (
        <div className="space-y-1">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Recipe Arbitrage Parameters
          </Typography>
          <div className="mt-2 space-y-1">
            <ConfigRow
              label="Min Margin Percent"
              value={data.recipeArbitrageConfig.minMarginPercent}
            />
          </div>
        </div>
      )}

      {data.components && data.components.length > 0 && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Components
          </Typography>
          {data.components.map((c, i) => (
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
