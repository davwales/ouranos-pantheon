import { Typography } from "@/components/shared/typography";
import { type StrategyDetail } from "@/lib/api/plutus";
import { ConfigRow } from "./config-row";
import { INPUT_KIND_LABELS, THRESHOLD_FIELDS } from "./strategy-constants";

export function StrategyConfigurationView({ data }: { data: StrategyDetail }) {
  const activeWeights = data.inputWeights
    .filter((w) => w.weight !== 0)
    .sort((a, b) => b.weight - a.weight);

  const hasThresholds = THRESHOLD_FIELDS.some(
    (f) => data.thresholds[f.key] != null,
  );

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

      {activeWeights.length > 0 && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Input Weights
          </Typography>
          <div className="space-y-1">
            {activeWeights.map((w) => (
              <ConfigRow
                key={w.kind}
                label={INPUT_KIND_LABELS[w.kind]}
                value={w.weight}
              />
            ))}
          </div>
        </div>
      )}

      {hasThresholds && (
        <div className="space-y-2">
          <Typography
            variant="small"
            className="font-semibold uppercase tracking-wide text-muted-foreground"
          >
            Thresholds
          </Typography>
          <div className="space-y-1">
            {THRESHOLD_FIELDS.map((f) => (
              <ConfigRow
                key={f.key}
                label={f.label}
                value={data.thresholds[f.key]}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
